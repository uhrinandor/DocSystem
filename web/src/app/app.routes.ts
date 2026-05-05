import { Routes } from '@angular/router';
import { AgentWorkflowPageComponent } from './pages/agent-workflow-page.component';

export const routes: Routes = [
  {
    path: '',
    component: AgentWorkflowPageComponent,
  },
  {
    path: '**',
    redirectTo: '',
  },
];
