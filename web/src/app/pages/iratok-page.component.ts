import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';

import { IratService } from '../api/generated/irat/irat.service';
import type { Irat } from '../api/generated/model/irat';

@Component({
  selector: 'app-iratok-page',
  imports: [CommonModule],
  templateUrl: './iratok-page.component.html',
  styleUrl: './iratok-page.component.css',
})
export class IratokPageComponent {
  private readonly iratService = inject(IratService);

  protected readonly iratok = signal<Irat[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal('');

  constructor() {
    this.loadIratok();
  }

  protected reload(): void {
    this.loadIratok();
  }

  private loadIratok(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.iratService.getApiIrat('application/json').subscribe({
      next: (result) => {
        if (typeof result === 'string') {
          this.iratok.set([]);
          this.errorMessage.set('Az iratok listaja nem JSON formatumban erkezett vissza.');
          this.isLoading.set(false);
          return;
        }

        this.iratok.set(result);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.iratok.set([]);
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.trim()) {
        return error.error;
      }

      if (error.error?.message) {
        return error.error.message;
      }

      return `Az iratok betoltese sikertelen volt (${error.status || 'ismeretlen hiba'}).`;
    }

    return 'Az iratok betoltese sikertelen volt.';
  }
}
