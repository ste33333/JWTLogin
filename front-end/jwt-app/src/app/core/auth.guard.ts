import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../core/auth.service';

export const authGuard: CanActivateFn = (route, state): Observable<boolean | UrlTree> | boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (isLoggedIn && !authService.isTokenExpired()) {
        return true;
      } else {
        authService.logout();
        return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
      }
    })
  );
};