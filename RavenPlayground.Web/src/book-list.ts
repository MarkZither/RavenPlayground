import { EventAggregator } from 'aurelia-event-aggregator';
import { WebAPI } from './web-api';
import { BookUpdated, BookViewed } from './messages';
import { Book } from './book-detail';
import { inject } from 'aurelia-framework';

@inject(WebAPI, EventAggregator)
export class BookList {
  books;
 selectedId = 0;

  constructor(private api: WebAPI, ea: EventAggregator) {
    ea.subscribe(BookViewed, msg => this.select(msg.book));
    ea.subscribe(BookUpdated, msg => {
      let id = msg.book.bookId;
      let found = this.books.find(x => x.bookId == id);
      Object.assign(found, msg.book);
    });}

  created() {
    this.api.getBookList().then(books => this.books = books);
  }

  select(book) {
    this.selectedId = book.bookId;
    return true;
  }

  search(keywords) {
   this.api.searchBooks(keywords).then(books => {
      this.books = books;
      //selectedId = 0;
      //this.routeConfig.navModel.setTitle(this.book.title);
      //this.originalBook = JSON.parse(JSON.stringify(this.book));
      //this.ea.publish(new BookUpdated(this.book));
    });
  }

  get canSearch() {
    return !this.api.isRequesting;
  }
}
