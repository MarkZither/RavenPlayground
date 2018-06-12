import { EventAggregator } from 'aurelia-event-aggregator';
import { WebAPI } from './web-api';
import { BookUpdated, BookViewed } from './messages';
import { inject } from 'aurelia-framework';

@inject(WebAPI, EventAggregator)
export class BookList {
  books;
  selectedId = 0;

  constructor(private api: WebAPI, ea: EventAggregator) {
    ea.subscribe(BookViewed, msg => this.select(msg.book));
    ea.subscribe(BookUpdated, msg => {
      let id = msg.contact.id;
      let found = this.books.find(x => x.id == id);
      Object.assign(found, msg.contact);
    });}

  created() {
    this.api.getBookList().then(books => this.books = books);
  }

  select(book) {
    this.selectedId = book.id;
    return true;
  }
}
