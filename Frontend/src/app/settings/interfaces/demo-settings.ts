/**
 * Configuration settings for demo mode.
 */
export interface DemoSettings {
   /** Whether demo mode is active.*/
   isEnabled: boolean;
   /** The user identifier for the demo session. */
   userId: string;
   /** Token validity duration in minutes. Must be a positive integer. */
   tokenDurationInMinutes: number; 
}