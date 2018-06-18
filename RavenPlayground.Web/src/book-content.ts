import { inject } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import { WebAPI } from './web-api';
import { BookUpdated, BookViewed } from './messages';
import { Book } from './book-detail';
import { areEqual } from './utility';

@inject(WebAPI, EventAggregator)
export class BookContent {
  routeConfig;
  book: Book;
  originalBook: Book;

  constructor(private api: WebAPI, private ea: EventAggregator) { }
}
