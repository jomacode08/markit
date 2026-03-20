import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const CustomPreset = definePreset(Aura, {
    semantic: {
        primary: {
            50: '{indigo.50}',
            100: '{indigo.100}',
            200: '{indigo.200}',
            300: '{indigo.300}',
            400: '{indigo.400}',
            500: '{indigo.500}',
            600: '{indigo.600}',
            700: '{indigo.700}',
            800: '{indigo.800}',
            900: '{indigo.900}',
            950: '{indigo.950}'
        },
        colorScheme: {
            dark: {
                surface: {
                    0: '#ffffff',
                    50: ' #f0f2fa',
                    100: '#bbbec8',
                    200: '#8a8c95',
                    300: '#696b73',
                    400: '#54565d',
                    500: '#46484e',
                    600: '#3c3e43',
                    700: '#34363b',
                    800: '#2d2e32',
                    900: '#232429',
                    950: '#1a1b1e'
                },
            }
        }
    },
    components: {
        button: {
            // General styles
            root: {
                borderRadius: '0.375rem',
                sm: {
                    paddingY: '.5rem',
                    paddingX: '1rem',
                    fontSize: '.85rem',
                },
                lg: {
                    fontSize: '1rem',
                },
            },
            css: (dt) => `
                .p-button {
                    width: 100%;
                }
                .p-button-icon {
                    margin-right: .25rem;
                } 
            `,
            // Styles by color scheme
            colorScheme: {
                dark: {
                    root: {
                        primary: {
                            background: 'var(--highlight-surface-purple)',
                            borderColor: 'var(--highlight-border-purple)',
                            color: 'var(--text-color)',
                            // Hover
                            hoverBackground: 'var(--highlight-border-purple)',
                            hoverColor: 'var(--text-color)',
                            hoverBorderColor: 'var(--highlight-border-purple)'
                        },
                        secondary: {
                            background: 'transparent',
                            borderColor: 'var(--border-color)',
                            color: 'var(--text-color)',
                            // Hover
                            hoverBackground: '{surface.900}',
                            hoverColor: 'var(--text-color)',
                            hoverBorderColor: 'var(--border-color)'
                        },
                        success: {
                            background: 'var(--highlight-surface-green)',
                            borderColor: 'var(--highlight-border-green)',
                            color: 'var(--text-color)',
                            // Hover
                            hoverBackground: 'var(--highlight-border-green)',
                            hoverColor: 'var(--text-color)',
                            hoverBorderColor: 'var(--highlight-border-green)'
                        },
                        danger: {
                            background: 'var(--highlight-surface-red)',
                            borderColor: 'var(--highlight-border-red)',
                            color: 'var(--text-color)',
                            // Hover
                            hoverBackground: 'var(--highlight-border-red)',
                            hoverColor: 'var(--text-color)',
                            hoverBorderColor: 'var(--highlight-border-red)'
                        },
                        info: {
                            background: 'var(--highlight-surface-blue)',
                            borderColor: 'var(--highlight-border-blue)',
                            color: 'var(--text-color)',
                            // Hover
                            hoverBackground: 'var(--highlight-border-blue)',
                            hoverColor: 'var(--text-color)',
                            hoverBorderColor: 'var(--highlight-border-blue)'
                        }
                    },
                    text: {
                        secondary: {
                            // Hover
                            color: 'var(--text-foreground)',
                            hoverBackground: '{surface.800}',
                        }
                    }
                }
            },
        },
        dialog: {
            root: {
                background: '{surface.950}',
                borderColor: '{surface.700}',
                borderRadius: '6px',
            },
            header: {
                padding: '1rem',
            },
        },
        divider: {
            content: {
                background: '{surface.950}',
                color: 'var(--primary-text)'
            },
            css: () => `
                .p-divider-content {
                    font-weight: 500;
                }
            `
        },
        confirmdialog: {
            icon: {
                size: '3rem',
                color: 'var(--secondary-accent)'
            }
        },
        dataview: {
            content: {
                background: 'transparent'
            }
        },
        datepicker: {
            css: () => `
                .p-datepicker-panel {
                    min-width: 0px !important;
                }
                .p-datepicker-next-button,
                .p-datepicker-prev-button {
                    height: 25px !important;
                    width: 25px !important;
                }
            `
        },
        drawer: {
            header: {
                padding: '1rem'
            },
            css: (dt) => `
                .p-drawer-content {
                    display: flex;
                    padding: 0 1rem 1rem 1rem;
                }
            `
        },
        inputtext: {
            root: {
                invalidBorderColor: '{red.500}',
                paddingX: '.75rem',
                paddingY: '.5rem',
            },
            colorScheme: {
                dark: {
                    root: {
                        background: '{surface.900}',
                        borderColor: '{surface.700}',
                        placeholderColor: '{surface.300}'
                    }
                }
            },
            css: (dt) => `
                .p-inputtext {
                    width: 100%;
                    font-size: .875rem;
                }
            `
        },
        inputnumber: {
            css: (dt) => `
                .p-inputnumber {
                    width: 100%;
                }
            `
        },
        menu: {
            list: {
                padding: '0',
            },
            item: {
                borderRadius: '0px',
            },
            colorScheme: {
                dark: {
                    item: {
                        icon: {
                            color: 'var(--text-secondary)',
                            focusColor: 'var(--text-primary)'
                        }
                    }
                }
            }
        },
        multiselect: {
            root: {
                background: '{surface.900}',
                invalidBorderColor: '{red.500}',
            },
            overlay: {
                background: '{surface.900}',
            },
            list: {
                header: {
                    padding: '.5rem 1rem'
                }
            },
            css: ({dt}) => `
                .p-multiselect-header {
                    border-bottom: 1px solid ${dt('surface.700')};
                }
                .p-multiselect-option {
                    font-size: .875rem;
                }
            `
        },
        paginator: {
            root: {
                background: 'transparent',
            },
            colorScheme: {
                dark: {
                    navButton: {
                        selectedBackground: 'var(--highlight-surface-purple)'
                    }
                }
            },
        },
        panel: {
            content: {
                padding: '0px'
            },
            css: ({dt}) => `
                .p-panel-header-actions {
                    display: flex;
                    gap: .25rem;
                    .p-button {
                        display:flex;
                        align-items: center;
                        justify-content: center;
                        padding: 1rem;
                        width: 25px;
                        height: 25px;
                    }
                }
            `
        },
        password: {
            css: ({dt}) => `
                .p-password-overlay  {
                    font-size: .875rem;
                } 
            `
        },
        progressspinner: {
            colorScheme: {
                dark: {
                    root: {
                        colorOne: '{primary.200}',
                        colorTwo: '{primary.300}',
                        colorThree: '{primary.400}',
                        colorFour: '{primary.500}',
                    }
                }
            }
        },
        radiobutton: {
            colorScheme: {
                dark : {
                    root: {
                        background: '{surface.900}',
                        borderColor: '{surface.700}',
                    }
                }
            }
        },
        skeleton: {
            root: {
                background: '{surface.900}',
                borderRadius: '0px'
            }
        },
        tooltip: {
            colorScheme: {
                dark: {
                    root: {
                        background: '{primary.400}',
                        color: '{primary.950}'
                    }
                }
            },
            css: () => `
                .p-tooltip {
                    font-size: .825rem;
                    font-weight: 400;
                }
            `
        },
        toggleswitch: {
            root: {
                width: '50px'
            }
        }
    }
});