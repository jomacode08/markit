export const NOTEBOOKS_STORAGE_KEY : string = 'local-notebooks';
export const ISAUTHENTICATED_STORAGE_KEY : string = 'is-authenticated'
export const DEFAULT_NOTEBOOK_NAME : string = 'Untitled'; 
export const DEFAULT_BLOCK_NAME : string = 'Main block';
export const MAIN_COLLECTION_PARAM : string = 'library';

export const ROUTES = {
    // Auth
    REDIRECT_GOOGLE: 'redirect-google',
    // Notebooks
    NOTEBOOKS_SEE: (id: number) => `/notebooks/see/${ id }`,
    // Workplace
    COLLECTION_SEE: (id: number) => `/workplace/explore/${ id }`,
    LIBRARY: `/workplace/explore/${ MAIN_COLLECTION_PARAM }`,
    STARRED: '/workplace/starred',
    RECENT: '/workplace/recent',
    // Settings
    ACCOUNTS: '/settings/accounts',
    DEMO_FORM: '/settings/demo',
    AUTHENTICATION_SETTINGS: '/settings/authentication',
    // Others
    NOT_FOUND: '/not-found',
    ERROR: '/error',
    HOME_URL: '/dashboard',
    PROFILE: '/dashboard/profile',
};

export const THEME = {
    LIGHT_THEME: 'Light',
    DARK_THEME: 'Dark',
    DARK_MODE_SELECTOR: 'markit-dark',
    THEME_PREFERENCE_KEY: 'theme-preference',
}

export const POPUP_NAMES = {
    SIGN_IN : 'sign-in',
    LINK_ACCOUNT : 'link-account'
}