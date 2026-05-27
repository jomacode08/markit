/**
 * Configuration settings for demo mode.
 */
export interface DemoSettings {
   /** Whether demo mode is active.*/
   isEnabled: boolean;
   /** Session duration in minutes. Must be a positive integer. */
   sessionDurationInMinutes: number; 
}