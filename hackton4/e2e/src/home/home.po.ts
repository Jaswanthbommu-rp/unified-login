import { browser, by, element, ExpectedConditions } from 'protractor';
import { environment } from '@environment';
export class HomePage {
  usernameField = element(by.id('username'));
  passwordField = element(by.id('password'));
  loginButton = element(by.css('button[type="submit"]'));

  login(userName = environment.testingUsername, password = environment.testingPassword) {
    browser.get('/');
    browser.wait(ExpectedConditions.presenceOf(this.usernameField));
    this.usernameField.sendKeys(userName);
    this.passwordField.sendKeys(password);
    this.loginButton.click();
  }
}
