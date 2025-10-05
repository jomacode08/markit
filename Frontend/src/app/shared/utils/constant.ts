export const TOKEN_STORAGE_KEY: string = 'markit-token';
export const MEILISEARCH_TOKEN_STORAGE_KEY: string = 'meili-token';
export const MARKS_STORAGE_KEY : string = 'local-marks';
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

export const MEILISEARCH = {
    COLLECTION_INDEX_UID : 'collections',
    COLLECTION_SEARCHABLE_ATTRIBUTE_NAME : 'name',
    MARK_INDEX_UID : 'marks',
    MARK_SEARCHABLE_ATTRIBUTE_NAME : 'name',
}
