import { Injectable, signal, effect, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type DietMode = 'all' | 'veg' | 'nonveg';

@Injectable({ providedIn: 'root' })
export class DietService {
  private platformId = inject(PLATFORM_ID);
  private get isBrowser() { return isPlatformBrowser(this.platformId); }

  mode = signal<DietMode>(
    this.isBrowser ? ((localStorage.getItem('dietMode') as DietMode) || 'all') : 'all'
  );

  constructor() {
    effect(() => {
      const m = this.mode();
      if (!this.isBrowser) return;
      document.body.classList.remove('theme-veg', 'theme-nonveg');
      if (m === 'veg')    document.body.classList.add('theme-veg');
      if (m === 'nonveg') document.body.classList.add('theme-nonveg');
      localStorage.setItem('dietMode', m);
    });
  }

  setMode(m: DietMode) { this.mode.set(m); }

  isVisible(dietType: string): boolean {
    const m = this.mode();
    if (m === 'all')    return true;
    if (m === 'veg')    return dietType === 'Veg' || dietType === 'Vegan';
    if (m === 'nonveg') return dietType === 'Non-Veg';
    return true;
  }
}
