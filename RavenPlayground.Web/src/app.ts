import { Router, RouterConfiguration } from 'aurelia-router';
import { inject, PLATFORM } from 'aurelia-framework';
import { WebAPI } from './web-api';

@inject(WebAPI)
export class App {
  router: Router;

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Books';
    config.map([
      { route: '', moduleId: PLATFORM.moduleName('no-selection'), title: 'Select' },
      { route: 'books/:bookId', moduleId: PLATFORM.moduleName('book-detail'), name: 'books' },
      { route: 'books/:bookId/read', moduleId: PLATFORM.moduleName('book-content'), name: 'book' }
    ]);

    this.router = router;
  }
    message = 'Hello World!';
}
