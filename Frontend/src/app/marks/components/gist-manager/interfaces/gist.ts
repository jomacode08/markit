export enum GistResponseStatus {
    success = 'success',
    failure = 'failure',
}

export interface GistResponse {
    status: GistResponseStatus,
    errorMessage?: string,
    gist?: Gist,
}

export interface Gist {
    id: string,
    url: string,
    title: string,
    description: string,
    author: string,
    createdAt: Date,
    files: GistFile[],
}

export interface GistFile {
    id: string,
    fileName: string,
    type: string,
    content: string,
    rawUrl: string,
    language?: string,
    html?: string,
}