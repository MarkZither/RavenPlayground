import { inject } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import { WebAPI } from './web-api';
import { BookUpdated, BookViewed } from './messages';
import { areEqual } from './utility';

export interface Book {
  bookId: number;
  title: string;
  author: string;
  language: string;
}

@inject(WebAPI, EventAggregator)
export class BookDetail {
  routeConfig;
  book: Book;
  originalBook: Book;

  constructor(private api: WebAPI, private ea: EventAggregator) { }

  activate(params, routeConfig) {
    this.routeConfig = routeConfig;

    return this.api.getBookDetails(params.bookId).then(book => {
      this.book = <Book>book;
      this.routeConfig.navModel.setTitle(this.book.title);
      this.originalBook = JSON.parse(JSON.stringify(this.book));
      this.ea.publish(new BookViewed(this.book));
    });
  }

  get canSave() {
    return this.book.title && this.book.author && this.book.language && this.book.bookId && !this.api.isRequesting;
  }

  save() {
    this.api.saveBook(this.book).then(book => {
      this.book = <Book>book;
      this.routeConfig.navModel.setTitle(this.book.title);
      this.originalBook = JSON.parse(JSON.stringify(this.book));
      this.ea.publish(new BookUpdated(this.book));
    });
  }

  canDeactivate() {
    if (!areEqual(this.originalBook, this.book)) {
      let result = confirm('You have unsaved changes. Are you sure you wish to leave?');

      if (!result) {
        this.ea.publish(new BookViewed(this.book));
      }

      return result;
    }

    return true;
  }
}
