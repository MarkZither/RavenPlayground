let latency = 200;
let id = 0;

function getId() {
  return ++id;
}

let books = [
  {
    id: getId(),
    firstName: 'John',
    lastName: 'Tolkien',
    email: 'tolkien@inklings.com',
    phoneNumber: '867-5309'
  },
  {
    id: getId(),
    firstName: 'Clive',
    lastName: 'Lewis',
    email: 'lewis@inklings.com',
    phoneNumber: '867-5309'
  },
  {
    id: getId(),
    firstName: 'Owen',
    lastName: 'Barfield',
    email: 'barfield@inklings.com',
    phoneNumber: '867-5309'
  },
  {
    id: getId(),
    firstName: 'Charles',
    lastName: 'Williams',
    email: 'williams@inklings.com',
    phoneNumber: '867-5309'
  },
  {
    id: getId(),
    firstName: 'Roger',
    lastName: 'Green',
    email: 'green@inklings.com',
    phoneNumber: '867-5309'
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
            id: x.id,
            firstName: x.firstName,
            lastName: x.lastName,
            email: x.email
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
        let found = books.filter(x => x.id == id)[0];
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
        let found = books.filter(x => x.id == book.id)[0];

        if (found) {
          let index = books.indexOf(found);
          books[index] = instance;
        } else {
          instance.id = getId();
          books.push(instance);
        }

        this.isRequesting = false;
        resolve(instance);
      }, latency);
    });
  }
}
