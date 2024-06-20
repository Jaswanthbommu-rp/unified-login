import { HomePage } from './home.po';
import { browser, element, by } from 'protractor';

describe('RealPage Sample App', () => {
  let page: HomePage;

  beforeEach(() => {
    page = new HomePage();
  });

  it('should have a title', () => {
    page.login();

    expect(element(by.css('.raul-header-product'))).toBeTruthy();
  });
});
