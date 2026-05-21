import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';

import { add } from './api-client';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly left = 1;
  protected readonly right = 2;
  protected readonly result = signal<number | string | undefined>(undefined);
  protected readonly error = signal<string | undefined>(undefined);

  async ngOnInit(): Promise<void> {
    const response = await add({
      query: {
        left: this.left,
        right: this.right,
      },
    });

    if (response.error) {
      this.error.set('Die Add-Funktion konnte nicht geladen werden.');
      return;
    }

    this.result.set(response.data?.result);
  }
}
