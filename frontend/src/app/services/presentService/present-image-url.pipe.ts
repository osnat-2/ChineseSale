import { Pipe, PipeTransform, inject } from '@angular/core';
import { HttpService } from '../httpService/http-service';

@Pipe({
  name: 'presentImageUrl',
  standalone: true,
})
export class PresentImageUrlPipe implements PipeTransform {
  private readonly apiOrigin = new URL(inject(HttpService).url).origin;

  transform(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'assets/present-placeholder.svg';
    }

    if (imageUrl.startsWith('/uploads/presents/')) {
      return new URL(imageUrl, this.apiOrigin).toString();
    }

    if (/^https?:\/\//i.test(imageUrl)) {
      try {
        const url = new URL(imageUrl);
        return url.hostname ? imageUrl : 'assets/present-placeholder.svg';
      } catch {
        return 'assets/present-placeholder.svg';
      }
    }

    return imageUrl;
  }
}
