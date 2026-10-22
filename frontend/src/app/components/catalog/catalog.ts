import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../i18n/translate.pipe';
import { categoryModel } from '../../models/category';
import { presentModel } from '../../models/present';
import { CategoryService } from '../../services/categoryService/category-service';
import { AuthService } from '../../services/authService/auth-service';
import { CardService } from '../../services/cardService/card-service';
import { PresentQuery, PresentService } from '../../services/presentService/present-service';
import { LocalizedCurrencyPipe } from '../../i18n/localized-currency.pipe';
import { ActivatedRoute, Params, Router, RouterLink } from '@angular/router';
import { hasApiResponseMessage } from '../../models/api-message';
import { PresentImageUrlPipe } from '../../services/presentService/present-image-url.pipe';

@Component({
  selector: 'app-catalog',
  imports: [CommonModule, FormsModule, TranslatePipe, LocalizedCurrencyPipe, RouterLink, PresentImageUrlPipe],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss'
})
export class Catalog {
  presents: presentModel[] = [];
  categories: categoryModel[] = [];
  search = '';
  private loadedSearch = '';
  categoryId = '';
  sortBy: PresentQuery['sortBy'] = 'name';
  sortDirection: PresentQuery['sortDirection'] = 'asc';
  loading = true;
  errorMessage = '';
  categoryErrorMessage = '';
  selectedPresent: presentModel | null = null;
  addingPresentId: number | null = null;
  quantities: Record<number, number | null> = {};
  addCardSuccessMessage = '';
  addCardErrorMessage = '';
  readonly maximumQuantity = 100;
  private addDialogTrigger: HTMLButtonElement | null = null;

  @ViewChild('quantityInput')
  set quantityInput(input: ElementRef<HTMLInputElement> | undefined) {
    input?.nativeElement.focus();
  }

  private readonly presentService = inject(PresentService);
  private readonly categoryService = inject(CategoryService);
  private readonly cardService = inject(CardService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly isAuthenticated = inject(AuthService).isAuthenticated;

  ngOnInit(): void {
    const query = this.route.snapshot.queryParamMap;
    this.search = query.get('search') ?? '';
    this.loadedSearch = this.search.trim();
    const categoryId = query.get('categoryId') ?? '';
    this.categoryId = Number.isInteger(Number(categoryId)) && Number(categoryId) > 0 ? categoryId : '';
    this.sortBy = query.get('sortBy') === 'price' ? 'price' : 'name';
    this.sortDirection = query.get('sortDirection') === 'desc' ? 'desc' : 'asc';
    this.loadCategories();
    this.loadPresents();
  }

  loadPresents(): void {
    this.loading = true;
    this.errorMessage = '';
    const query: PresentQuery = {
      categoryId: this.categoryId ? Number(this.categoryId) : undefined,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.presentService.getAllPresents(query).subscribe({
      next: (result) => {
        if (result.success) {
          this.presents = result.data ?? [];
          this.loadedSearch = this.search.trim();
          this.errorMessage = '';
        } else {
          this.errorMessage = this.presents.length ? 'catalog.refreshFailed' : 'catalog.loadFailed';
        }
        this.loading = false;
      },
      error: () => {
        this.errorMessage = this.presents.length ? 'catalog.refreshFailed' : 'catalog.loadFailed';
        this.loading = false;
      }
    });
  }

  clearFilters(): void {
    this.search = '';
    this.categoryId = '';
    this.sortBy = 'name';
    this.sortDirection = 'asc';
    this.loadPresents();
  }

  clearSearch(): void {
    this.search = '';
    this.loadPresents();
  }

  retryCategories(): void {
    this.loadCategories();
  }

  get hasContentFilters(): boolean {
    return Boolean(this.search.trim() || this.categoryId);
  }

  get filteredPresents(): presentModel[] {
    const query = this.loadedSearch.toLocaleLowerCase();
    if (!query) {
      return this.presents;
    }

    return this.presents.filter((present) => {
      const categoryName = present.categoryName ?? '';
      return [present.name, present.description, categoryName]
        .some((value) => value.toLocaleLowerCase().includes(query));
    });
  }

  get catalogQueryParams(): Params {
    return {
      search: this.search.trim() || null,
      categoryId: this.categoryId || null,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection,
    };
  }

  signInQueryParams(present: presentModel): Params {
    const returnUrl = this.router.serializeUrl(this.router.createUrlTree(
      ['/catalog', present.id],
      { queryParams: this.catalogQueryParams }
    ));
    return { returnUrl };
  }

  openAddDialog(present: presentModel, trigger: HTMLButtonElement): void {
    if (!this.isAuthenticated() || this.addingPresentId !== null) {
      return;
    }

    this.addDialogTrigger = trigger;
    this.selectedPresent = present;
    this.addCardErrorMessage = '';
    this.addCardSuccessMessage = '';
  }

  closeAddDialog(): void {
    if (this.addingPresentId !== null) {
      return;
    }

    this.selectedPresent = null;
    this.addCardErrorMessage = '';
    this.restoreAddDialogFocus();
  }

  handleAddDialogKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeAddDialog();
    }
  }

  addPresentToBasket(present: presentModel): void {
    const quantity = this.quantityFor(present.id);
    if (
      !this.isAuthenticated() ||
      this.selectedPresent?.id !== present.id ||
      this.addingPresentId !== null ||
      !this.isValidQuantity(quantity)
    ) {
      return;
    }

    this.addingPresentId = present.id;
    this.addCardSuccessMessage = '';
    this.addCardErrorMessage = '';

    this.cardService.addCard({ presentId: present.id, quantity }).subscribe({
      next: (response) => {
        this.addCardSuccessMessage = hasApiResponseMessage(response) ? '' : 'card.addQuantitySuccess';
        this.selectedPresent = null;
        this.addingPresentId = null;
        this.restoreAddDialogFocus(true);
      },
      error: () => {
        this.addCardErrorMessage = 'card.addFailed';
        this.addingPresentId = null;
      }
    });
  }

  private restoreAddDialogFocus(afterRender = false): void {
    const trigger = this.addDialogTrigger;
    this.addDialogTrigger = null;
    if (afterRender) {
      setTimeout(() => trigger?.focus());
    } else {
      trigger?.focus();
    }
  }

  quantityFor(presentId: number): number | null {
    return this.quantities[presentId] === undefined ? 1 : this.quantities[presentId];
  }

  isQuantityValid(presentId: number): boolean {
    return this.isValidQuantity(this.quantityFor(presentId));
  }

  private isValidQuantity(quantity: number | null): quantity is number {
    return typeof quantity === 'number' &&
      Number.isInteger(quantity) &&
      quantity >= 1 &&
      quantity <= this.maximumQuantity;
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (result) => {
        this.categories = result.success ? result.data ?? [] : [];
        this.categoryErrorMessage = result.success ? '' : 'catalog.categoriesUnavailable';
      },
      error: () => {
        this.categories = [];
        this.categoryErrorMessage = 'catalog.categoriesUnavailable';
      },
    });
  }

  imageFallback(event: Event): void {
    const image = event.target as HTMLImageElement;
    image.src = 'assets/present-placeholder.svg';
  }
}