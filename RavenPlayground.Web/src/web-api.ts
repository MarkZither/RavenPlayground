let latency = 200;
let id = 0;

function getId() {
  return ++id;
}

let books = [
  {
    bookId: getId(),
    author: 'John',
    title: 'Tolkien',
    language: 'en'
  },
  {
    bookId: getId(),
    author:  'Clive',
    title: 'Lewis',
    language: 'en'
  },
  {
    bookId: getId(),
    author: 'Owen',
    title: 'Barfield',
    language: 'fr'
  },
  {
    bookId: getId(),
    author: 'Charles',
    title: 'Williams',
    language: 'de'
  },
  {
    bookId: getId(),
    author: 'Roger',
    title: 'Green',
    language: 'en'
  }
];

let searchBooks = [
  {
    bookId: getId(),
    author: 'SearchJohn',
    title: 'Tolkien',
    language: 'en'
  },
  {
    bookId: getId(),
    author: 'SearchClive',
    title: 'Lewis',
    language: 'en'
  },
  {
    bookId: getId(),
    author: 'SearchOwen',
    title: 'Barfield',
    language: 'fr'
  },
  {
    bookId: getId(),
    author: 'SearchCharles',
    title: 'Williams',
    language: 'de'
  },
  {
    bookId: getId(),
    author: 'SearchRoger',
    title: 'Green',
    language: 'en'
  }
];

export class WebAPI {
  isRequesting = false;

  getBookList() {
    this.isRequesting = true;
    return new Promise(resolve => {
      setTimeout(() => {
        let results = books.map(x => {
          return {
            bookId: x.bookId,
            title: x.title,
            author: x.author,
            language: x.language
          }
        });
        resolve(results);
        this.isRequesting = false;
      }, latency);
    });
  }

  getBookDetails(id) {
    this.isRequesting = true;
    return new Promise(resolve => {
      setTimeout(() => {
        let found = books.filter(x => x.bookId == id)[0];
        resolve(JSON.parse(JSON.stringify(found)));
        this.isRequesting = false;
      }, latency);
    });
  }

  saveBook(book) {
    this.isRequesting = true;
    return new Promise(resolve => {
      setTimeout(() => {
        let instance = JSON.parse(JSON.stringify(book));
        let found = books.filter(x => x.bookId == book.bookId)[0];

        if (found) {
          let index = books.indexOf(found);
          books[index] = instance;
        } else {
          instance.bookId = getId();
          books.push(instance);
        }

        this.isRequesting = false;
        resolve(instance);
      }, latency);
    });
  }

  searchBooks(keywords) {
    this.isRequesting = true;
    return new Promise(resolve => {
      setTimeout(() => {
        let results = searchBooks.map(x => {
          return {
            bookId: x.bookId,
            title: x.title,
            author: x.author,
            language: x.language
          }
        });
        resolve(results);
        books = searchBooks;
        this.isRequesting = false;
      }, latency);
    });
  }
}
