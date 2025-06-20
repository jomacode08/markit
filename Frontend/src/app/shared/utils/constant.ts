export const TOKEN_STORAGE_KEY: string = 'markit-token';
export const DEFAULT_MARK_NAME : string = 'My new mark 🎉'; 
export const DEFAULT_BLOCK_NAME : string = 'Main block';
export const MAIN_COLLECTION_PARAM : string = 'my-marks';

export const ROUTES = {
    // Marks
    MARKS_NEW: 'marks/new',
    MARKS_SEE: (id: number) => `marks/see/${ id }`,
    
    // Workplace
    COLLECTION_SEE: (id: number) => `workplace/explore/${ id }`,
    MY_MARKS: `workplace/explore/${ MAIN_COLLECTION_PARAM }`,
    STARRED: 'workplace/starred',
    RECENT: 'workplace/recent',
    
    // Others
    NOT_FOUND: 'not-found',
    HOME_URL: 'dashboard'
};
