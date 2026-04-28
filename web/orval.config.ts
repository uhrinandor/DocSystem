import { defineConfig } from 'orval';

export default defineConfig({
  docSystem: {
    input: {
      target: '../DocSystem/openapi/DocSystem.json',
    },
    output: {
      target: 'src/app/api/generated/doc-system.ts',
      schemas: 'src/app/api/generated/model',
      client: 'angular',
      mode: 'tags-split',
    },
  },
});
