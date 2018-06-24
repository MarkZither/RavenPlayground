import { HttpClient } from 'aurelia-fetch-client';

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
        
        let httpClient = new HttpClient();
        httpClient.configure(config => {
          config
            .withBaseUrl('api/')
            .withDefaults({
              credentials: 'same-origin',
              headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'Fetch'
              }
            })
            .withInterceptor({
              request(request) {
                console.log(`Requesting ${request.method} ${request.url}`);
                return request;
              },
              response(response) {
                console.log(`Received ${response.status} ${response.url}`);
                return response;
              }
            });
        });

        httpClient.fetch('GutBook')
          .then(response => response.json())
          .then(data => {
            console.log(data[0].title);
            books = data;
            let results = books.map(x => {
              //let results = books.map(x => {
              return {
                bookId: x.bookId,
                title: x.title,
                author: x.author,
                language: x.language
              }
            });
            resolve(results);
            this.isRequesting = false;
          });
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
