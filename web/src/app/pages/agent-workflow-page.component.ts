import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { AgentRunResult, OpenApiAgentService } from '../services/openapi-agent.service';

@Component({
  selector: 'app-agent-workflow-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './agent-workflow-page.component.html',
  styleUrl: './agent-workflow-page.component.css',
})
export class AgentWorkflowPageComponent {
  private readonly formBuilder = new FormBuilder();
  private readonly openApiAgent = inject(OpenApiAgentService);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly runResult = signal<AgentRunResult | null>(null);

  protected readonly corePrompt = `Magyar iratkezelesi workflow agent vagy. A user termeszetes nyelvu kereseibol pontos API lepeseket tervezz. Csak a kapott OpenAPI muveleteket hasznald. Ne talalj ki endpointot vagy mezot. Ha egy lepeshez korabbi valasz adata kell, azt mentsd el es hasznald tovabb. Ha a user uzleti azonosito mezot ad, ne kerj rogton id-t, hanem elobb keresd meg a rekordot. Ha nincs kulon GET vegpont egy entitasra, de a szukseges adat egy masik GET valaszaba beagyazva elerheto, azt hasznald.`;

  protected readonly domainPrompts: Record<string, string> = {
    iktatokonyv: `Iktatokonyv: evhez es kodhoz tartozo nyilvantartas. Fontos mezok: id, nev, kod, evszam, createdAt, iktatoszamok. Legujabbat createdAt alapjan valaszd. Kod vagy nev alapjan elobb keresd meg, utana hasznald az id-t. Az iktatoszamok listaja az iktatokonyv GET valaszaban beagyazva is elerheto, ezert meglvo iktatoszamok lekerdezesehez nem kotelezo kulon /api/Iktatas GET endpoint.`,

    iktatas: `Iktatas: foszam uj ugyet nyit, alszam meglevo ugyet bovit. Az iktatoszam egy iktatokonyvhöz tartozik. Uj ugy inditasakor altalaban foszamos iktatas kell, meglevo ugy dokumentumahoz alszamos. Foszamos iktatoszam az, ahol nincs alszam vagy az alszam 1. Alszamos iktatoszam az, ahol az alszam 2 vagy nagyobb. Ha a user alszamos iktatoszamokat ker, csak az alszam >= 2 elemeket add vissza. Egy olyan ertek, mint 0003/1-AI20262/2026, nem kod, hanem szoveges iktatoszam, ezert ilyen esetben szovegesIktatoszam vagy iktatoszam.szovegesIktatoszam alapjan kell keresni.`,

    ugyirat: `Ugyirat: egy ugy osszefogo rekordja. Jellemzoen foszamos iktatassal jon letre, es tobb irat tartozhat hozza. Az ugyirat ala tartozo iratok az irats listaban vannak. Ha a user azt kerdezi, hany irat tartozik egy ugyirathoz, akkor az irats lista elemszamat kell venni. Ha a user egy ugyiratot szoveges iktatoszammal azonosit, akkor a megfelelo ugyiratot az iktatoszam.szovegesIktatoszam alapjan kell megkeresni. Ha a user ugyiratok iktatoszamait keresi egy iktatokonyvben, azt az iktatokonyvhoz tartozo beagyazott iktatoszamokbol is meg lehet allapitani.`,

    irat: `Irat: konkret dokumentum vagy bejegyzes. Altalaban egy ugyirathoz tartozik, es sajat iktatoszama is lehet. Egy ugyirat ala tartozo iratok szama az ugyirat irats listajanak darabszama.`,

    decisionRules: `Ne keverd az ugyiratot az irattal. Ha id kell, de csak kod vagy nev adott, elobb keress. Ha a user iktatoszam formatumu erteket ad, azt ne kodkent kezeld, hanem szoveges iktatoszamkent. Ugyirat alatti iratok szamat az ugyirat irats listajanak hossza adja. Csak bizonyithato mezot hasznalj. Főszámos kerésnél a foszamos vagy alszam nelkuli/1-es alszamu teteleket ertsd. Alszámos kerésnél csak a 2 vagy nagyobb alszamu teteleket vedd figyelembe. Ha nincs kulon vegpont az adatra, de egy masik GET valaszaban beagyazva szerepel, onnan olvasd ki. A valasz csak a kert strukturalt kimenet legyen.`,
  };

  protected readonly promptCatalog = [
    { key: 'core', title: 'Core prompt', content: this.corePrompt },
    { key: 'iktatokonyv', title: 'Iktatókönyv domain', content: this.domainPrompts['iktatokonyv'] },
    { key: 'iktatas', title: 'Iktatás domain', content: this.domainPrompts['iktatas'] },
    { key: 'ugyirat', title: 'Ügyirat domain', content: this.domainPrompts['ugyirat'] },
    { key: 'irat', title: 'Irat domain', content: this.domainPrompts['irat'] },
    { key: 'decisionRules', title: 'Döntési szabályok', content: this.domainPrompts['decisionRules'] },
  ];

  protected readonly agentRules = [
    'Az agent csak az ismert rendszerfunkciókat és API műveleteket használhatja.',
    'Hiányzó vagy bizonytalan adat esetén előbb pontosítást kell kérnie, nem találgathat.',
    'Minden művelet előtt a workflow szövegét strukturált feladatlépésekre kell bontania.',
    'Többlépéses feladatnál a korábbi API válaszokból származó adatokat a következő lépésekben is fel kell használnia.',
    'A válaszban röviden össze kell foglalnia, hogy mit végzett el és mi maradt függőben.',
  ];

  protected readonly workflowForm = this.formBuilder.nonNullable.group({
    apiKey: ['', [Validators.required]],
    workflowDescription: ['', [Validators.required, Validators.minLength(12)]],
    clerkNote: [''],
  });

  protected async submit(): Promise<void> {
    if (this.workflowForm.invalid || this.isSubmitting()) {
      this.workflowForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.runResult.set(null);

    try {
      const formValue = this.workflowForm.getRawValue();
      const result = await this.openApiAgent.run({
        apiKey: formValue.apiKey,
        workflowDescription: formValue.workflowDescription,
        corePrompt: this.corePrompt,
        domainPrompts: this.domainPrompts,
        systemInstructions: this.agentRules,
      });

      this.runResult.set(result);
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Az agent futtatása sikertelen volt.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
