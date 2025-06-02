export const TOKEN_STORAGE_KEY: string = 'markit-token';

export const ROUTES = {
    // Marks
    MARKS_DEFAULT: 'marks/',
    MARKS_NEW: 'marks/new',
    MARKS_SEE: (id: number) => `marks/see/${ id }`,
    
    // Collections
    COLLECTION_EXPLORER: 'collections',
    COLLECTIONS_SEE: (id: number) => `collections/${ id }`,
    
    // Others
    NOT_FOUND: 'not-found',
    HOME_URL: 'dashboard'
};

export const DEFAULT_MARK_NAME : string = 'My new mark 🎉'; 
export const DEFAULT_BLOCK_NAME : string = 'Main block';