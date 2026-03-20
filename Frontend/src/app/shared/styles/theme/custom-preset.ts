import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const CustomPreset = definePreset(Aura, {
    primitive: {
        surkit: {
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
    },
    semantic: {
        primary: {
            50: '{violet.50}',
            100: '{violet.100}',
            200: '{violet.200}',
            300: '{violet.300}',
            400: '{violet.400}',
            500: '{violet.500}',
            600: '{violet.600}',
            700: '{violet.700}',
            800: '{violet.800}',
            900: '{violet.900}',
            950: '{violet.950}'
        },
        colorScheme: {
            dark: {
                surface: {
                    0: '{surkit.0}',
                    50: '{surkit.50}',
                    100: '{surkit.100}',
                    200: '{surkit.200}',
                    300: '{surkit.300}',
                    400: '{surkit.400}',
                    500: '{surkit.500}',
                    600: '{surkit.600}',
                    700: '{surkit.700}',
                    800: '{surkit.800}',
                    900: '{surkit.900}',
                    950: '{surkit.950}'
                },
                highlight: {
                    primary: {
                        background: 'color-mix(in srgb, {primary.500}, transparent 85%)',
                        border: 'color-mix(in srgb, {primary.500}, transparent 70%)',
                    },
                    info: {
                        background: 'color-mix(in srgb, {blue.500}, transparent 85%)',
                        border: 'color-mix(in srgb, {blue.500}, transparent 70%)',
                    },
                    success: {
                        background: 'color-mix(in srgb, {green.500}, transparent 85%)',
                        border: 'color-mix(in srgb, {green.500}, transparent 70%)',
                    },
                    warning: {
                        background: 'color-mix(in srgb, {amber.500}, transparent 85%)',
                        border: 'color-mix(in srgb, {amber.500}, transparent 70%)',
                    },
                    danger: {
                        background: 'color-mix(in srgb, {red.500}, transparent 85%)',
                        border: 'color-mix(in srgb, {red.500}, transparent 70%)',
                    },
                }
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
                            background: '{highlight.primary.background}',
                            color: '{primary.100}',
                            borderColor: '{highlight.primary.border}',
                            // Hover
                            hoverBackground: 'color-mix(in srgb, {primary.500}, transparent 70%)',
                            hoverColor: '{primary.100}',
                            hoverBorderColor: 'color-mix(in srgb, {primary.500}, transparent 70%)',
                            // Active
                            activeBackground: 'color-mix(in srgb, {primary.500}, transparent 60%)',
                            activeColor: '{primary.200}',
                            activeBorderColor: 'color-mix(in srgb, {primary.500}, transparent 70%)'
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
                            background: '{highlight.success.background}',
                            color: '{green.100}',
                            borderColor: '{highlight.success.border}',
                            // Hover
                            hoverBackground: 'color-mix(in srgb, {green.500}, transparent 70%)',
                            hoverColor: '{green.100}',
                            hoverBorderColor: 'color-mix(in srgb, {green.500}, transparent 70%)',
                            // Active
                            activeBackground: 'color-mix(in srgb, {green.500}, transparent 60%)',
                            activeColor: '{green.200}',
                            activeBorderColor: 'color-mix(in srgb, {green.500}, transparent 70%)'
                        },
                        danger: {
                            background: '{highlight.danger.background}',
                            color: '{red.100}',
                            borderColor: '{highlight.danger.border}',
                            // Hover
                            hoverBackground: 'color-mix(in srgb, {red.500}, transparent 70%)',
                            hoverColor: '{red.100}',
                            hoverBorderColor: 'color-mix(in srgb, {red.500}, transparent 70%)',
                            // Active
                            activeBackground: 'color-mix(in srgb, {red.500}, transparent 60%)',
                            activeColor: '{red.200}',
                            activeBorderColor: 'color-mix(in srgb, {red.500}, transparent 70%)'
                        },
                        info: {
                            background: '{highlight.info.background}',
                            color: '{blue.100}',
                            borderColor: '{highlight.info.border}',
                            // Hover
                            hoverBackground: 'color-mix(in srgb, {blue.500}, transparent 70%)',
                            hoverColor: '{blue.100}',
                            hoverBorderColor: 'color-mix(in srgb, {blue.500}, transparent 70%)',
                            // Active
                            activeBackground: 'color-mix(in srgb, {blue.500}, transparent 60%)',
                            activeColor: '{blue.200}',
                            activeBorderColor: 'color-mix(in srgb, {blue.500}, transparent 70%)'
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
                        selectedBackground: 'var(--highlight-primary-background)'
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