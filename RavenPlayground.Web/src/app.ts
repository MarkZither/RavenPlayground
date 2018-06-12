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
      { route: 'books/:id', moduleId: PLATFORM.moduleName('book-detail'), name: 'books' }
    ]);

    this.router = router;
  }
    message = 'Hello World!';
}
