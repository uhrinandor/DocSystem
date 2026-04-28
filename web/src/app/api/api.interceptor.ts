import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_ROOT_URL } from './api-root.token';

const isAbsoluteUrl = (url: string): boolean => /^https?:\/\//i.test(url);

const joinUrl = (rootUrl: string, requestUrl: string): string =>
  `${rootUrl.replace(/\/+$/, '')}/${requestUrl.replace(/^\/+/, '')}`;

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const rootUrl = inject(API_ROOT_URL);

  if (!rootUrl || isAbsoluteUrl(req.url)) {
    return next(req);
  }

  return next(
    req.clone({
      url: joinUrl(rootUrl, req.url),
    }),
  );
};
