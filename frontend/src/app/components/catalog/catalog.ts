import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { categoryModel } from '../../models/category';
import { presentModel } from '../../models/present';
import { CategoryService } from '../../services/categoryService/category-service';
import { PresentQuery, PresentService } from '../../services/presentService/present-service';

@Component({
  selector: 'app-catalog',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss'
})
export class Catalog {
  presents: presentModel[] = [];
  categories: categoryModel[] = [];
  search = '';
  categoryId = '';
  sortBy: PresentQuery['sortBy'] = 'name';
  sortDirection: PresentQuery['sortDirection'] = 'asc';
  loading = true;
  errorMessage = '';

  private readonly presentService = inject(PresentService);
  private readonly categoryService = inject(CategoryService);

  ngOnInit(): void {
    this.loadCategories();
    this.loadPresents();
  }

  loadPresents(): void {
    this.loading = true;
    this.errorMessage = '';
    const query: PresentQuery = {
      search: this.search.trim(),
      categoryId: this.categoryId ? Number(this.categoryId) : undefined,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.presentService.getAllPresents(query).subscribe({
      next: (result) => {
        this.presents = result.success ? result.data ?? [] : [];
        this.errorMessage = result.success ? '' : result.message || 'Unable to load the catalogue.';
        this.loading = false;
      },
      error: () => {
        this.presents = [];
        this.errorMessage = 'Unable to load the catalogue. Please try again.';
        this.loading = false;
      }
    });
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (result) => {
        this.categories = result.success ? result.data ?? [] : [];
      }
    });
  }

  imageFallback(event: Event): void {
    const image = event.target as HTMLImageElement;
    image.src = 'assets/images/present-placeholder.svg';
  }
}