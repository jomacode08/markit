export const MARKS_STORAGE_KEY : string = 'local-marks';
export const ISAUTHENTICATED_STORAGE_KEY : string = 'is-authenticated'
export const DEFAULT_MARK_NAME : string = 'My new mark 🎉'; 
export const DEFAULT_BLOCK_NAME : string = 'Main block';
export const MAIN_COLLECTION_PARAM : string = 'my-marks';

export const ROUTES = {
    // Auth
    REDIRECT_GOOGLE: 'redirect-google',
    // Marks
    MARKS_SEE: (id: number) => `/marks/see/${ id }`,
    // Workplace
    COLLECTION_SEE: (id: number) => `/workplace/explore/${ id }`,
    MY_MARKS: `/workplace/explore/${ MAIN_COLLECTION_PARAM }`,
    STARRED: '/workplace/starred',
    RECENT: '/workplace/recent',
    // Settings
    ACCOUNTS: '/settings/accounts',
    DEMO_FORM: '/settings/demo',
    // Others
    NOT_FOUND: '/not-found',
    ERROR: '/error',
    HOME_URL: '/dashboard',
    PROFILE: '/dashboard/profile',
};

export const POPUP_NAMES = {
    SIGN_IN : 'sign-in',
    LINK_ACCOUNT : 'link-account'
}