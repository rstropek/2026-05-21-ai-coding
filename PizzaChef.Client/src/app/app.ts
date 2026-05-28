import { ChangeDetectionStrategy, Component, OnInit, computed, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  getMenu,
  getOrderingWindow,
  placeOrder,
  type Menu,
  type MenuCategory,
  type MenuItem,
  type OrderingWindow,
} from './api-client';

interface CartLine {
  key: string;
  itemId: string;
  variantId: string | null;
  label: string;
  unitPrice: number;
  quantity: number;
}

interface CategoryGroup {
  category: MenuCategory;
  items: MenuItem[];
}

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  private readonly fb = new FormBuilder();

  protected readonly menu = signal<Menu | undefined>(undefined);
  protected readonly window = signal<OrderingWindow | undefined>(undefined);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly searchTerm = signal('');
  private readonly variantSelections = signal<Record<string, string>>({});
  protected readonly cart = signal<CartLine[]>([]);

  protected readonly submitError = signal<string | undefined>(undefined);
  protected readonly submitSuccess = signal(false);

  protected readonly orderForm = this.fb.group({
    employeeName: ['', [Validators.required, Validators.minLength(2)]],
    notes: [''],
  });

  protected readonly filteredCategories = computed<CategoryGroup[]>(() => {
    const m = this.menu();
    if (!m) {
      return [];
    }
    const term = this.searchTerm().trim().toLowerCase();
    const itemsFiltered = term
      ? m.items.filter((i) => i.name.toLowerCase().includes(term))
      : m.items;

    return [...m.categories]
      .sort((a, b) => this.asNumber(a.sortOrder) - this.asNumber(b.sortOrder))
      .map((c) => ({
        category: c,
        items: itemsFiltered.filter((i) => i.categoryId === c.id),
      }))
      .filter((g) => g.items.length > 0);
  });

  protected readonly cartTotal = computed(() =>
    this.cart().reduce((sum, l) => sum + l.unitPrice * l.quantity, 0),
  );

  protected readonly canSubmit = computed(() => {
    return (
      this.window()?.isOpen === true &&
      this.cart().length > 0 &&
      this.orderForm.valid
    );
  });

  async ngOnInit(): Promise<void> {
    await Promise.all([this.loadMenu(), this.loadWindow()]);
  }

  protected onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
  }

  protected selectedVariant(itemId: string): string {
    const sel = this.variantSelections()[itemId];
    if (sel) {
      return sel;
    }
    const item = this.menu()?.items.find((i) => i.id === itemId);
    return item?.variants[0]?.id ?? '';
  }

  protected onVariantChange(itemId: string, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.variantSelections.update((m) => ({ ...m, [itemId]: value }));
  }

  protected addToCart(item: MenuItem): void {
    const variantId =
      item.variants.length > 0 ? this.selectedVariant(item.id) : null;
    const variant = item.variants.find((v) => v.id === variantId);
    const unitPrice =
      this.asNumber(item.price) + this.asNumber(variant?.priceDelta ?? 0);
    const label = variant ? `${item.name} (${variant.name})` : item.name;
    const key = `${item.id}|${variantId ?? ''}`;

    this.cart.update((lines) => {
      const existing = lines.find((l) => l.key === key);
      if (existing) {
        return lines.map((l) =>
          l.key === key ? { ...l, quantity: l.quantity + 1 } : l,
        );
      }
      return [
        ...lines,
        {
          key,
          itemId: item.id,
          variantId,
          label,
          unitPrice,
          quantity: 1,
        },
      ];
    });
    this.submitSuccess.set(false);
  }

  protected increment(key: string): void {
    this.cart.update((lines) =>
      lines.map((l) => (l.key === key ? { ...l, quantity: l.quantity + 1 } : l)),
    );
  }

  protected decrement(key: string): void {
    this.cart.update((lines) =>
      lines
        .map((l) =>
          l.key === key ? { ...l, quantity: Math.max(0, l.quantity - 1) } : l,
        )
        .filter((l) => l.quantity > 0),
    );
  }

  protected remove(key: string): void {
    this.cart.update((lines) => lines.filter((l) => l.key !== key));
  }

  protected async submit(): Promise<void> {
    this.submitError.set(undefined);
    this.submitSuccess.set(false);

    if (!this.canSubmit()) {
      this.orderForm.markAllAsTouched();
      return;
    }

    const lines = this.cart();
    const payload = {
      employeeName: this.orderForm.value.employeeName!.trim(),
      items: lines.map((l) => ({
        itemId: l.itemId,
        variantId: l.variantId,
        quantity: l.quantity,
      })),
      notes: this.orderForm.value.notes?.trim() || null,
    };

    const response = (await placeOrder({ body: payload })) as {
      data?: unknown;
      error?: { detail?: string; title?: string } | unknown;
      response?: { ok?: boolean; status?: number; error?: unknown };
    };
    const ok = response.response?.ok === true || response.error === undefined;
    if (!ok) {
      const body =
        (response.error as { detail?: string; title?: string } | undefined) ??
        (response.response?.error as { detail?: string; title?: string } | undefined);
      const detail =
        body?.detail ??
        body?.title ??
        'Bestellung konnte nicht gespeichert werden.';
      this.submitError.set(detail);
      return;
    }

    this.submitSuccess.set(true);
    this.cart.set([]);
    this.orderForm.reset({ employeeName: '', notes: '' });
    await this.loadWindow();
  }

  protected formatPrice(amount: number): string {
    return new Intl.NumberFormat('de-AT', {
      style: 'currency',
      currency: this.menu()?.currency ?? 'EUR',
    }).format(amount);
  }

  protected formatPriceDelta(amount: number): string {
    if (amount === 0) {
      return '±0,00';
    }
    const sign = amount > 0 ? '+' : '−';
    return `${sign}${this.formatPrice(Math.abs(amount))}`;
  }

  protected formatTime(iso: string): string {
    const d = new Date(iso);
    return new Intl.DateTimeFormat('de-AT', {
      hour: '2-digit',
      minute: '2-digit',
    }).format(d);
  }

  protected asNumber(value: number | string | null | undefined): number {
    if (value === null || value === undefined) {
      return 0;
    }
    return typeof value === 'number' ? value : Number(value);
  }

  private async loadMenu(): Promise<void> {
    const response = await getMenu();
    if (response.error || !response.data) {
      this.loadError.set('Speisekarte konnte nicht geladen werden.');
      return;
    }
    this.menu.set(response.data);
  }

  private async loadWindow(): Promise<void> {
    const response = await getOrderingWindow();
    if (response.error || !response.data) {
      this.loadError.set('Bestellfenster konnte nicht ermittelt werden.');
      return;
    }
    this.window.set(response.data);
  }
}
