import { Injectable } from '@angular/core';

import { environment } from '../../environments/environment';

interface OpenApiSchema {
  type?: string;
  properties?: Record<string, OpenApiSchema>;
  items?: OpenApiSchema;
  required?: string[];
  enum?: string[];
  $ref?: string;
  description?: string;
}

interface OpenApiParameter {
  name: string;
  in: 'path' | 'query' | 'header' | 'cookie';
  required?: boolean;
  description?: string;
  schema?: OpenApiSchema;
}

interface OpenApiOperation {
  summary?: string;
  description?: string;
  parameters?: OpenApiParameter[];
  requestBody?: {
    required?: boolean;
    content?: Record<string, { schema?: OpenApiSchema }>;
  };
  responses?: Record<
    string,
    {
      description?: string;
      content?: Record<string, { schema?: OpenApiSchema }>;
    }
  >;
}

interface OpenApiSpec {
  paths: Record<string, Partial<Record<'get' | 'post' | 'put' | 'patch' | 'delete', OpenApiOperation>>>;
  components?: {
    schemas?: Record<string, OpenApiSchema>;
  };
}

interface PlannedStepAuth {
  userName?: string;
  password?: string;
}

interface PlannedOperationStep {
  auth?: PlannedStepAuth;
  method?: string;
  path?: string;
  query?: Record<string, unknown>;
  body?: Record<string, unknown>;
  reason?: string;
  saveAs?: string;
}

interface PlannedWorkflow {
  summary?: string;
  steps?: PlannedOperationStep[];
  needsClarification?: string;
}

interface GroqChatCompletionResponse {
  choices?: Array<{
    message?: {
      content?: string | null;
    };
  }>;
}

interface StepExecutionResult {
  status: number;
  ok: boolean;
  data: unknown;
}

type TemplateContext = Record<string, unknown>;
type DomainKey = 'iktatokonyv' | 'iktatas' | 'ugyirat' | 'irat' | 'user' | 'decisionRules';

interface PromptSelection {
  domains: DomainKey[];
  activePrompt: string;
  specSummary: string;
}

export interface AgentRunResult {
  finalResult: string;
  operationLine: string;
  executionSummary: string;
  rawResponse: string;
  activityLog: string[];
  activeDomains: string[];
  activePrompt: string;
  specSummary: string;
}

export interface AgentRunInput {
  apiKey: string;
  workflowDescription: string;
  corePrompt: string;
  domainPrompts: Record<string, string>;
  systemInstructions: string[];
}

@Injectable({ providedIn: 'root' })
export class OpenApiAgentService {
  private specCache: OpenApiSpec | null = null;
  private readonly backendTokenCache = new Map<string, string>();

  async run(input: AgentRunInput): Promise<AgentRunResult> {
    const spec = await this.loadSpec();
    const activityLog: string[] = ['OpenAPI spec betöltve.'];
    const promptSelection = this.buildPromptSelection(spec, input.workflowDescription, input);
    activityLog.push(`Aktiv domain blokkok: ${promptSelection.domains.join(', ')}.`);
    activityLog.push('Csak a relevans OpenAPI osszefoglalo lett elkuldve a modellnek.');

    const plannedWorkflow = await this.planWorkflow(input, promptSelection);

    if (plannedWorkflow.needsClarification) {
      activityLog.push(`Pontosítás szükséges: ${plannedWorkflow.needsClarification}`);
      return {
        finalResult: plannedWorkflow.needsClarification,
        operationLine: 'Nincs végrehajtott művelet',
        executionSummary: plannedWorkflow.needsClarification,
        rawResponse: JSON.stringify(plannedWorkflow),
        activityLog,
        activeDomains: promptSelection.domains,
        activePrompt: promptSelection.activePrompt,
        specSummary: promptSelection.specSummary,
      };
    }

    if (!plannedWorkflow.steps || plannedWorkflow.steps.length === 0) {
      throw new Error('A modell nem adott vissza végrehajtható lépéssort.');
    }

    const context: TemplateContext = {};
    const operationLines: string[] = [];
    const executionSummaries: Array<Record<string, unknown>> = [];

    activityLog.push(`Tervezett lépések száma: ${plannedWorkflow.steps.length}`);

    for (const [index, rawStep] of plannedWorkflow.steps.entries()) {
      const step = this.resolveTemplates(rawStep, context) as PlannedOperationStep;
      this.validateStep(step, index);

      const stepPrefix = `Lépés ${index + 1}`;
      const operationLine = this.formatOperationLine(step);
      operationLines.push(operationLine);
      activityLog.push(`${stepPrefix}: ${operationLine}`);

      if (step.reason) {
        activityLog.push(`${stepPrefix} indoklás: ${step.reason}`);
      }

      const backendToken = await this.loginToBackend(step.auth);
      activityLog.push(
        `${stepPrefix}: backend authentikáció sikeres (${step.auth?.userName ?? environment.agentBackendUserName}).`,
      );

      const execution = await this.executeOperation(step, backendToken);
      activityLog.push(`${stepPrefix}: API hívás lefutott.`);
      activityLog.push(`${stepPrefix}: HTTP státusz ${execution.status}.`);

      executionSummaries.push({
        step: index + 1,
        operation: operationLine,
        status: execution.status,
        ok: execution.ok,
        data: execution.data,
      });

      if (!execution.ok) {
        const finalResult = this.buildFinalResult(input.workflowDescription, plannedWorkflow, executionSummaries);
        activityLog.push('Olvasható vegeredmeny osszefoglalva.');
        return {
          finalResult,
          operationLine: operationLines.join('\n'),
          executionSummary: JSON.stringify(
            {
              summary: plannedWorkflow.summary ?? null,
              completedSteps: executionSummaries,
              failedStep: index + 1,
            },
            null,
            2,
          ),
          rawResponse: JSON.stringify(plannedWorkflow, null, 2),
          activityLog,
          activeDomains: promptSelection.domains,
          activePrompt: promptSelection.activePrompt,
          specSummary: promptSelection.specSummary,
        };
      }

      if (step.saveAs) {
        context[step.saveAs] = execution.data;
        activityLog.push(`${stepPrefix}: mentett változó -> ${step.saveAs}.`);
      }
    }

    const finalResult = this.buildFinalResult(input.workflowDescription, plannedWorkflow, executionSummaries);
    activityLog.push('Olvasható vegeredmeny osszefoglalva.');

    return {
      finalResult,
      operationLine: operationLines.join('\n'),
      executionSummary: JSON.stringify(
        {
          summary: plannedWorkflow.summary ?? null,
          completedSteps: executionSummaries,
        },
        null,
        2,
      ),
      rawResponse: JSON.stringify(plannedWorkflow, null, 2),
      activityLog,
      activeDomains: promptSelection.domains,
      activePrompt: promptSelection.activePrompt,
      specSummary: promptSelection.specSummary,
    };
  }

  private async loadSpec(): Promise<OpenApiSpec> {
    if (this.specCache) {
      return this.specCache;
    }

    const response = await fetch(environment.openApiSpecUrl);

    if (!response.ok) {
      throw new Error('Nem sikerült betölteni az OpenAPI spec fájlt.');
    }

    const spec = (await response.json()) as OpenApiSpec;
    this.specCache = spec;
    return spec;
  }

  private buildPromptSelection(spec: OpenApiSpec, workflowDescription: string, input: AgentRunInput): PromptSelection {
    const domains = this.selectDomains(workflowDescription);
    const activePrompt = [input.corePrompt, ...domains.map((domain) => input.domainPrompts[domain]).filter(Boolean)].join(
      '\n\n',
    );
    const specSummary = this.buildSpecSummary(spec, domains);

    return { domains, activePrompt, specSummary };
  }

  private selectDomains(workflowDescription: string): DomainKey[] {
    const text = workflowDescription.toLowerCase();
    const domains = new Set<DomainKey>(['decisionRules']);

    if (/(iktat[óo]k[öo]nyv|k[óo]d|evszam|createdat|legutobbi|legujabb)/.test(text)) {
      domains.add('iktatokonyv');
    }

    if (/(f[őo]sz[aá]m|alsz[aá]m|iktat|uj ugy|inditsunk egy ugyet)/.test(text)) {
      domains.add('iktatas');
      domains.add('ugyirat');
    }

    if (/(irat|dokumentum)/.test(text)) {
      domains.add('irat');
    }

    if (/(ugyirat|ugy)\b/.test(text)) {
      domains.add('ugyirat');
    }

    if (/(felhaszn[aá]l[oó]|user|bejelentkez)/.test(text)) {
      domains.add('user');
    }

    if (domains.size === 1) {
      domains.add('iktatokonyv');
      domains.add('iktatas');
    }

    return Array.from(domains);
  }

  private buildSpecSummary(spec: OpenApiSpec, domains: DomainKey[]): string {
    const lines: string[] = [];

    for (const [path, methods] of Object.entries(spec.paths)) {
      if (!this.pathMatchesDomains(path, domains)) {
        continue;
      }

      for (const [method, operation] of Object.entries(methods)) {
        if (!operation) {
          continue;
        }

        const op = operation as OpenApiOperation;
        const parameters = (op.parameters ?? [])
          .filter((parameter) => parameter.in !== 'cookie')
          .map((parameter) => {
            const schema = this.resolveSchema(spec, parameter.schema);
            return `${parameter.in}:${parameter.name}${parameter.required ? '*' : ''}:${schema.type ?? 'string'}`;
          });

        const bodySchema = op.requestBody?.content?.['application/json']?.schema;
        const body = bodySchema ? this.describeBody(spec, bodySchema) : 'none';
        const responses = this.describeResponses(spec, op);

        lines.push(
          [
            `${method.toUpperCase()} ${path}`,
            parameters.length > 0 ? `params=[${parameters.join(', ')}]` : 'params=[]',
            `body=${body}`,
            `responses=${responses}`,
          ]
          .filter(Boolean)
          .join(' | '),
        );
      }
    }

    return lines.join('\n');
  }

  private pathMatchesDomains(path: string, domains: DomainKey[]): boolean {
    const domainMatchers: Record<DomainKey, RegExp> = {
      iktatokonyv: /\/api\/Iktatokonyv/i,
      iktatas: /\/api\/Iktatas/i,
      ugyirat: /\/api\/Ugyirat/i,
      irat: /\/api\/Irat/i,
      user: /\/api\/User/i,
      decisionRules: /^$/,
    };

    return domains.some((domain) => domainMatchers[domain]?.test(path));
  }

  private describeBody(spec: OpenApiSpec, schema: OpenApiSchema): string {
    return this.describeSchema(spec, schema, 0, true);
  }

  private describeResponses(spec: OpenApiSpec, operation: OpenApiOperation): string {
    const entries = Object.entries(operation.responses ?? {});

    if (entries.length === 0) {
      return 'none';
    }

    return entries
      .map(([status, response]) => {
        const jsonSchema =
          response.content?.['application/json']?.schema ??
          response.content?.['text/json']?.schema ??
          response.content?.['text/plain']?.schema;

        const described = jsonSchema ? this.describeSchema(spec, jsonSchema, 0, false) : 'none';
        return `${status}:${described}`;
      })
      .join(', ');
  }

  private describeSchema(spec: OpenApiSpec, schema: OpenApiSchema, depth: number, includeAllFields: boolean): string {
    const resolved = this.resolveSchema(spec, schema);
    const maxDepth = includeAllFields ? 2 : 1;

    if (depth >= maxDepth) {
      if (Array.isArray(resolved.type)) {
        return resolved.type.join('|');
      }

      return resolved.type ?? 'object';
    }

    if (Array.isArray(resolved.type)) {
      return resolved.type.join('|');
    }

    if (resolved.enum && resolved.enum.length > 0) {
      return `enum(${resolved.enum.join('|')})`;
    }

    if (resolved.type === 'array' && resolved.items) {
      return `array<${this.describeSchema(spec, resolved.items, depth + 1, includeAllFields)}>`;
    }

    if (resolved.type !== 'object' || !resolved.properties) {
      return resolved.type ?? 'unknown';
    }

    const entries = Object.entries(resolved.properties);
    const prioritizedKeys = [
      'id',
      'kod',
      'nev',
      'createdAt',
      'updatedAt',
      'evszam',
      'subject',
      'details',
      'iktatokonyvId',
      'iktatoszamok',
      'iktatoszam',
      'irats',
      'userName',
      'name',
      'token',
      'expiresAt',
    ];
    const relevantEntries = includeAllFields
      ? entries
      : entries
          .filter(([key]) => prioritizedKeys.includes(key))
          .sort(
            ([left], [right]) => prioritizedKeys.indexOf(left) - prioritizedKeys.indexOf(right),
          )
          .slice(0, 5);

    return `{${relevantEntries
      .map(([key, value]) => {
        const propertySchema = this.describeSchema(spec, value, depth + 1, includeAllFields);
        const required = resolved.required?.includes(key) ? '*' : '';
        return `${key}${required}:${propertySchema}`;
      })
      .join(', ')}}`;
  }

  private resolveSchema(spec: OpenApiSpec, schema?: OpenApiSchema): OpenApiSchema {
    if (!schema) {
      return { type: 'string' };
    }

    if (schema.$ref) {
      const schemaName = schema.$ref.split('/').pop();

      if (!schemaName) {
        return { type: 'string' };
      }

      return this.resolveSchema(spec, spec.components?.schemas?.[schemaName]);
    }

    if (schema.type === 'object' && schema.properties) {
      return {
        ...schema,
        properties: Object.fromEntries(
          Object.entries(schema.properties).map(([key, value]) => [key, this.resolveSchema(spec, value)]),
        ),
      };
    }

    if (schema.type === 'array' && schema.items) {
      return {
        ...schema,
        items: this.resolveSchema(spec, schema.items),
      };
    }

    return schema;
  }

  private async planWorkflow(input: AgentRunInput, promptSelection: PromptSelection): Promise<PlannedWorkflow> {
    const response = await fetch(`${environment.groqBaseUrl}/chat/completions`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${input.apiKey}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        model: environment.groqModel,
        temperature: 0.1,
        messages: [
          {
            role: 'system',
            content: [
              'You are an API workflow planner.',
              'You must ONLY use the provided OpenAPI operations. Never hallucinate endpoints.',
              'Return one minified JSON object only.',
              'Schema: {"summary":"...","steps":[{"auth":{"userName":"...","password":"..."},"method":"POST","path":"/api/X","query":{},"body":{},"saveAs":"x"}]} or {"needsClarification":"..."}.',
              'Use multiple ordered steps when needed.',
              'Use saveAs + {{var.id}} for values from earlier responses.',
              'List lookup syntax allowed: {{books.$match(kod=AI20262).id}}.',
              'If business field like kod or nev is given, plan a lookup before id-based action.',
              'If the user provides a textual case number like 0003/1-AI20262/2026, treat it as szovegesIktatoszam, not as kod.',
              'Use createdAt for latest/newest when available.',
              'If needed data is already present in an existing GET response as an embedded collection, use that response instead of asking for a missing dedicated endpoint.',
              'Never include /api/Auth/login as a step.',
              'Use auth only if the user explicitly gave credentials for that step.',
              'Keep body/query empty objects when unused.',
              promptSelection.activePrompt,
              ...input.systemInstructions,
            ].join('\n'),
          },
          {
            role: 'user',
            content: [
              `Relevant domains:\n${promptSelection.domains.join(', ')}`,
              `OpenAPI summary:\n${promptSelection.specSummary}`,
              `Workflow:\n${input.workflowDescription}`,
              'Example:',
              '{"summary":"Find by code then open case.","steps":[{"method":"GET","path":"/api/Iktatokonyv","query":{},"body":{},"saveAs":"books"},{"method":"POST","path":"/api/Iktatas/foszamos","query":{},"body":{"iktatokonyvId":"{{books.$match(kod=AI20262).id}}","subject":"Nagyon fontos ügy","details":""}}]}',
              '{"summary":"Find a case by textual number, then register under it.","steps":[{"method":"GET","path":"/api/Ugyirat","query":{},"body":{},"saveAs":"ugyiratList"},{"method":"POST","path":"/api/Iktatas/alszamos","query":{},"body":{"ugyiratId":"{{ugyiratList.$match(szovegesIktatoszam=0003/1-AI20262/2026).id}}","subject":"tesztelési tárgy","details":""}}]}',
              'Example for reading existing values from an embedded list:',
              '{"summary":"Return existing case numbers from the matching book.","steps":[{"method":"GET","path":"/api/Iktatokonyv","query":{},"body":{},"saveAs":"books"}]}',
            ].join('\n\n'),
          },
        ],
      }),
    });

    const rawText = await response.text();
    const payload = this.tryParseJson(rawText);

    if (!response.ok) {
      throw new Error(
        typeof payload === 'object' && payload !== null && 'error' in payload
          ? JSON.stringify(payload)
          : `A Groq API kérés sikertelen volt (${response.status}).`,
      );
    }

    const completion = payload as GroqChatCompletionResponse;
    const content = completion.choices?.[0]?.message?.content?.trim();

    if (!content) {
      throw new Error('A Groq modell nem adott vissza tartalmat.');
    }

    const parsed = this.tryParseJson(content);

    if (!parsed || typeof parsed !== 'object') {
      throw new Error(`A modell válasza nem feldolgozható JSON: ${content}`);
    }

    return parsed as PlannedWorkflow;
  }

  private buildFinalResult(
    workflowDescription: string,
    plannedWorkflow: PlannedWorkflow,
    executionSummaries: Array<Record<string, unknown>>,
  ): string {
    const lastStep = executionSummaries.at(-1);
    const failedStep = executionSummaries.find((step) => step['ok'] === false);

    if (failedStep) {
      return `A workflow nem futott le teljesen. A ${failedStep['step']}. lepes hibaval allt meg.`;
    }

    if (!lastStep) {
      return plannedWorkflow.summary ?? 'A workflow nem hajtott vegre muveletet.';
    }

    const data = lastStep['data'];
    const dataSummary = this.summarizeData(data, workflowDescription);
    if (dataSummary) {
      return `${plannedWorkflow.summary ?? 'A workflow sikeresen lefutott.'} ${dataSummary}`;
    }

    return plannedWorkflow.summary ?? 'A workflow sikeresen lefutott.';
  }

  private summarizeData(data: unknown, workflowDescription: string): string {
    const normalizedWorkflow = workflowDescription.toLowerCase();
    const requestedCode = this.extractRequestedCode(workflowDescription);
    const requestedTextualIktatoszam = this.extractRequestedTextualIktatoszam(workflowDescription);
    const asksForCount =
      normalizedWorkflow.includes('hány') ||
      normalizedWorkflow.includes('hany') ||
      normalizedWorkflow.includes('darab') ||
      normalizedWorkflow.includes('száma') ||
      normalizedWorkflow.includes('szama');
    const asksForListing =
      normalizedWorkflow.includes('listáz') ||
      normalizedWorkflow.includes('listaz') ||
      normalizedWorkflow.includes('sorold fel') ||
      normalizedWorkflow.includes('add vissza az elérhető') ||
      normalizedWorkflow.includes('add vissza az elerheto');
    const asksForCaseBookCodes =
      asksForListing &&
      (normalizedWorkflow.includes('iktatókönyv') || normalizedWorkflow.includes('iktatokonyv')) &&
      (
        normalizedWorkflow.includes('kódjait') ||
        normalizedWorkflow.includes('kodjait') ||
        normalizedWorkflow.includes('kódokat') ||
        normalizedWorkflow.includes('kodokat') ||
        normalizedWorkflow.includes('elérhető iktatókönyvek kódjait') ||
        normalizedWorkflow.includes('elerheto iktatokonyvek kodjait')
      );
    const asksForIktatoszamValues =
      normalizedWorkflow.includes('iktatószám') ||
      normalizedWorkflow.includes('iktatoszam');
    const asksForCaseBookDetails =
      (normalizedWorkflow.includes('iktatókönyv') || normalizedWorkflow.includes('iktatokonyv')) &&
      (normalizedWorkflow.includes('azonosítóját') ||
        normalizedWorkflow.includes('azonositojat') ||
        normalizedWorkflow.includes('nevét') ||
        normalizedWorkflow.includes('nevet') ||
        normalizedWorkflow.includes('évszámát') ||
        normalizedWorkflow.includes('evszamat'));

    if (asksForCaseBookCodes) {
      const codes = this.collectCaseBookCodes(data);
      if (codes.length > 0) {
        return `Az elerheto iktatokonyv kodok: ${codes.join(', ')}.`;
      }
    }

    if (asksForCaseBookDetails && requestedCode && Array.isArray(data)) {
      const target = this.findRecordByCode(data, requestedCode);
      if (target) {
        const parts = [
          `A ${requestedCode} kodu iktatokonyv`,
          target['id'] ? `azonositoja: ${String(target['id'])}` : '',
          target['nev'] ? `neve: ${String(target['nev'])}` : '',
          target['evszam'] !== undefined ? `evszama: ${String(target['evszam'])}` : '',
        ].filter(Boolean);

        return `${parts.join(', ')}.`;
      }
    }

    if (
      asksForCount &&
      (normalizedWorkflow.includes('ügyirat') || normalizedWorkflow.includes('ugyirat'))
    ) {
      const targetRecord =
        Array.isArray(data) && requestedTextualIktatoszam
          ? this.findRecordByTextualIktatoszam(data, requestedTextualIktatoszam)
          : !Array.isArray(data) && data && typeof data === 'object'
            ? (data as Record<string, unknown>)
            : null;

      if (targetRecord) {
        const irats = targetRecord['irats'];
        if (Array.isArray(irats)) {
          const prefix = requestedTextualIktatoszam
            ? `A ${requestedTextualIktatoszam} iktatoszamu ugyirat ala`
            : 'Az ugyirat ala';
          return `${prefix} ${irats.length} darab irat tartozik.`;
        }
      }
    }

    if (asksForIktatoszamValues && !asksForCount) {
      const target = Array.isArray(data) && requestedCode ? this.findRecordByCode(data, requestedCode) ?? data : data;
      const iktatoszamok = this.collectIktatoszamStrings(
        target,
        normalizedWorkflow.includes('alszámos') || normalizedWorkflow.includes('alszamos')
          ? 'alszamos'
          : normalizedWorkflow.includes('főszámos') || normalizedWorkflow.includes('foszamos')
            ? 'foszamos'
            : 'all',
      );

      if (iktatoszamok.length > 0) {
        const prefix = requestedCode
          ? `A ${requestedCode} kodu iktatokonyvben talalt iktatoszamok:`
          : 'A talalt iktatoszamok:';
        return `${prefix} ${iktatoszamok.join(', ')}.`;
      }
    }

    if (!data || typeof data !== 'object') {
      return '';
    }

    if (Array.isArray(data)) {
      return `A lekerdezes ${data.length} elemet adott vissza.`;
    }

    const record = data as Record<string, unknown>;
    const parts = [
      record['id'] ? `Azonosito: ${String(record['id'])}.` : '',
      record['kod'] ? `Kod: ${String(record['kod'])}.` : '',
      record['nev'] ? `Nev: ${String(record['nev'])}.` : '',
      record['subject'] ? `Targy: ${String(record['subject'])}.` : '',
    ].filter(Boolean);

    if ('iktatoszam' in record && record['iktatoszam'] && typeof record['iktatoszam'] === 'object') {
      const iktatoszam = record['iktatoszam'] as Record<string, unknown>;
      if (iktatoszam['szovegesIktatoszam']) {
        parts.push(`Iktatoszam: ${String(iktatoszam['szovegesIktatoszam'])}.`);
      }
    }

    return parts.join(' ');
  }

  private collectCaseBookCodes(data: unknown): string[] {
    const values = new Set<string>();

    if (Array.isArray(data)) {
      for (const item of data) {
        if (this.isCaseBookRecord(item)) {
          values.add(String((item as Record<string, unknown>)['kod']));
        }
      }
      return Array.from(values);
    }

    if (this.isCaseBookRecord(data)) {
      values.add(String((data as Record<string, unknown>)['kod']));
    }

    return Array.from(values);
  }

  private isCaseBookRecord(data: unknown): boolean {
    if (!data || typeof data !== 'object' || Array.isArray(data)) {
      return false;
    }

    const record = data as Record<string, unknown>;
    return (
      typeof record['kod'] === 'string' &&
      (typeof record['nev'] === 'string' || Array.isArray(record['iktatoszamok'])) &&
      record['evszam'] !== undefined
    );
  }

  private extractRequestedCode(workflowDescription: string): string | null {
    const codeMatch = workflowDescription.match(/\b[A-Z]{2,}\d{2,}\b/);
    return codeMatch ? codeMatch[0] : null;
  }

  private extractRequestedTextualIktatoszam(workflowDescription: string): string | null {
    const match = workflowDescription.match(/\b\d+\/\d+[-A-Z0-9/]+\b/i);
    return match ? match[0] : null;
  }

  private findRecordByCode(data: unknown[], code: string): Record<string, unknown> | null {
    const record = data.find(
      (entry) =>
        !!entry &&
        typeof entry === 'object' &&
        'kod' in entry &&
        String((entry as Record<string, unknown>)['kod']) === code,
    );

    return record && typeof record === 'object' ? (record as Record<string, unknown>) : null;
  }

  private findRecordByTextualIktatoszam(data: unknown[], textualIktatoszam: string): Record<string, unknown> | null {
    const record = data.find(
      (entry) =>
        !!entry &&
        typeof entry === 'object' &&
        this.matchesEntryByField(entry as Record<string, unknown>, 'szovegesIktatoszam', textualIktatoszam),
    );

    return record && typeof record === 'object' ? (record as Record<string, unknown>) : null;
  }

  private collectIktatoszamStrings(data: unknown, mode: 'all' | 'foszamos' | 'alszamos'): string[] {
    const values = new Set<string>();
    this.walkForIktatoszamok(data, values, mode);
    return Array.from(values);
  }

  private walkForIktatoszamok(data: unknown, values: Set<string>, mode: 'all' | 'foszamos' | 'alszamos'): void {
    if (!data) {
      return;
    }

    if (Array.isArray(data)) {
      for (const item of data) {
        this.walkForIktatoszamok(item, values, mode);
      }
      return;
    }

    if (typeof data !== 'object') {
      return;
    }

    const record = data as Record<string, unknown>;

    if (record['szovegesIktatoszam']) {
      const alszamValue = this.parseAlszam(record['alszam']);
      if (this.matchesIktatoszamMode(alszamValue, mode)) {
        values.add(String(record['szovegesIktatoszam']));
      }
    } else if (record['foszam'] !== undefined) {
      const foszam = String(record['foszam']);
      const alszamValue = this.parseAlszam(record['alszam']);
      if (this.matchesIktatoszamMode(alszamValue, mode)) {
        const alszam = alszamValue !== null ? `/${String(alszamValue)}` : '';
        values.add(`${foszam}${alszam}`);
      }
    }

    for (const value of Object.values(record)) {
      this.walkForIktatoszamok(value, values, mode);
    }
  }

  private parseAlszam(value: unknown): number | null {
    if (value === undefined || value === null || value === '') {
      return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private matchesIktatoszamMode(alszam: number | null, mode: 'all' | 'foszamos' | 'alszamos'): boolean {
    if (mode === 'all') {
      return true;
    }

    if (mode === 'foszamos') {
      return alszam === null || alszam <= 1;
    }

    return alszam !== null && alszam >= 2;
  }

  private async loginToBackend(auth?: PlannedStepAuth): Promise<string> {
    const userName = auth?.userName ?? environment.agentBackendUserName;
    const password = auth?.password ?? environment.agentBackendPassword;
    const cacheKey = `${userName}::${password}`;

    const cachedToken = this.backendTokenCache.get(cacheKey);
    if (cachedToken) {
      return cachedToken;
    }

    const response = await fetch(`${environment.apiRootUrl}/api/Auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify({
        userName,
        password,
      }),
    });

    if (!response.ok) {
      throw new Error(`A DocSystem API backend bejelentkezése sikertelen volt (${userName}).`);
    }

    const payload = (await response.json()) as { token?: string };

    if (!payload.token) {
      throw new Error('A DocSystem API nem adott vissza backend tokent.');
    }

    this.backendTokenCache.set(cacheKey, payload.token);
    return payload.token;
  }

  private async executeOperation(
    operation: PlannedOperationStep,
    backendToken: string,
  ): Promise<StepExecutionResult> {
    let url = `${environment.apiRootUrl}${operation.path}`;

    for (const [key, value] of Object.entries(operation.query ?? {})) {
      if (url.includes(`{${key}}`)) {
        url = url.replace(`{${key}}`, encodeURIComponent(String(value)));
      }
    }

    const remainingQuery = new URLSearchParams();
    for (const [key, value] of Object.entries(operation.query ?? {})) {
      if (!operation.path?.includes(`{${key}}`) && value !== undefined && value !== null) {
        remainingQuery.set(key, String(value));
      }
    }

    if (remainingQuery.size > 0) {
      url = `${url}?${remainingQuery.toString()}`;
    }

    const normalizedMethod = operation.method?.toUpperCase() ?? 'GET';
    const hasBody =
      !!operation.body &&
      Object.keys(operation.body).length > 0 &&
      normalizedMethod !== 'GET' &&
      normalizedMethod !== 'DELETE';

    const response = await fetch(url, {
      method: normalizedMethod,
      headers: {
        Authorization: `Bearer ${backendToken}`,
        Accept: 'application/json',
        ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
      },
      body: hasBody ? JSON.stringify(operation.body) : undefined,
    });

    const rawText = await response.text();
    const payload = this.tryParseJson(rawText);

    return {
      status: response.status,
      ok: response.ok,
      data: payload,
    };
  }

  private validateStep(step: PlannedOperationStep, index: number): void {
    if (!step.method || !step.path) {
      throw new Error(`A modell ${index + 1}. lépése nem tartalmaz végrehajtható method/path értéket.`);
    }

    step.method = step.method.toUpperCase();

    if (!step.query) {
      step.query = {};
    }

    if (!step.body && step.method !== 'GET' && step.method !== 'DELETE') {
      step.body = {};
    }
  }

  private resolveTemplates(value: unknown, context: TemplateContext): unknown {
    if (typeof value === 'string') {
      const exactMatch = value.match(/^{{\s*([^}]+?)\s*}}$/);
      if (exactMatch) {
        return this.lookupContextValue(context, exactMatch[1]);
      }

      return value.replace(/{{\s*([^}]+?)\s*}}/g, (_match, expression) => {
        const resolved = this.lookupContextValue(context, expression);
        return resolved === undefined || resolved === null ? '' : String(resolved);
      });
    }

    if (Array.isArray(value)) {
      return value.map((entry) => this.resolveTemplates(entry, context));
    }

    if (value && typeof value === 'object') {
      return Object.fromEntries(
        Object.entries(value).map(([key, entry]) => [key, this.resolveTemplates(entry, context)]),
      );
    }

    return value;
  }

  private lookupContextValue(context: TemplateContext, expression: string): unknown {
    const path = expression.trim().split('.');
    let current: unknown = context;

    for (const segment of path) {
      const matchExpression = segment.match(/^\$match\(([^=]+)=(.+)\)$/);
      if (matchExpression) {
        if (!Array.isArray(current)) {
          throw new Error(`A modell tombi keresest vart, de nem tombre hivatkozott: ${expression}`);
        }

        const field = matchExpression[1].trim();
        const expectedValue = matchExpression[2].trim();
        const matchedEntry = current.find((entry) => {
          if (!entry || typeof entry !== 'object') {
            return false;
          }

          return this.matchesEntryByField(entry as Record<string, unknown>, field, expectedValue);
        });

        if (!matchedEntry) {
          throw new Error(`A modell nem talalt megfelelo elemet a listaban: ${expression}`);
        }

        current = matchedEntry;
        continue;
      }

      if (!current || typeof current !== 'object' || !(segment in current)) {
        throw new Error(`A modell ismeretlen hivatkozást adott vissza: ${expression}`);
      }

      current = (current as Record<string, unknown>)[segment];
    }

    return current;
  }

  private matchesEntryByField(entry: Record<string, unknown>, field: string, expectedValue: string): boolean {
    const directValue = entry[field];
    if (directValue !== undefined && String(directValue) === expectedValue) {
      return true;
    }

    const nestedPaths: Record<string, string[]> = {
      szovegesIktatoszam: ['iktatoszam.szovegesIktatoszam'],
      foszam: ['iktatoszam.foszam'],
      alszam: ['iktatoszam.alszam'],
      kod: ['iktatokonyv.kod', 'iktatoszam.iktatokonyv.kod'],
    };

    for (const path of nestedPaths[field] ?? []) {
      const resolved = this.resolveObjectPath(entry, path);
      if (resolved !== undefined && String(resolved) === expectedValue) {
        return true;
      }
    }

    if (field === 'kod' && this.looksLikeTextualIktatoszam(expectedValue)) {
      const textual = this.resolveObjectPath(entry, 'iktatoszam.szovegesIktatoszam');
      if (textual !== undefined && String(textual) === expectedValue) {
        return true;
      }
    }

    return false;
  }

  private resolveObjectPath(record: Record<string, unknown>, path: string): unknown {
    const segments = path.split('.');
    let current: unknown = record;

    for (const segment of segments) {
      if (!current || typeof current !== 'object' || !(segment in current)) {
        return undefined;
      }

      current = (current as Record<string, unknown>)[segment];
    }

    return current;
  }

  private looksLikeTextualIktatoszam(value: string): boolean {
    return /^\d+\/\d+-.+\/\d{4}$/.test(value);
  }

  private formatOperationLine(operation: PlannedOperationStep): string {
    const auth = operation.auth?.userName ? ` auth=${operation.auth.userName}` : '';
    const query =
      operation.query && Object.keys(operation.query).length > 0
        ? ` query=${JSON.stringify(operation.query)}`
        : '';
    const body =
      operation.body && Object.keys(operation.body).length > 0
        ? ` body=${JSON.stringify(operation.body)}`
        : '';
    const saveAs = operation.saveAs ? ` saveAs=${operation.saveAs}` : '';

    return `${operation.method} ${operation.path}${auth}${query}${body}${saveAs}`;
  }

  private tryParseJson(rawText: string): unknown {
    if (!rawText) {
      return null;
    }

    try {
      return JSON.parse(rawText) as unknown;
    } catch {
      return rawText;
    }
  }
}
