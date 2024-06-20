// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  loginAuthority: 'https://www-dev.realpage.com/login/identity/',
  unifiedLoginApiRoot: 'https://www-dev2.realpage.com/api',
  unifiedLoginApiErrors: 'http://controltower.corp.realpage.com:9099/goto/bacaea31fd9890792d0b9956dfd1be84',
  testingUsername: 'realpagead@test.com',
  testingPassword: 'P@ssw0rd',
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
