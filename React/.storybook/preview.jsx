/** @type { import('@storybook/react-vite').Preview } */
const preview = {
  loaders: [],
  parameters: {
    controls: {
      matchers: {
       color: /(background|color)$/i,
       date: /Date$/i,
      },
    },

    backgrounds: {
      options: {
        dark: { name: 'dark', value: 'rgb(35, 35, 35)' },
        light: { name: 'light', value: 'rgb(220, 220, 220)' }
      }
    },

    a11y: {
      // 'todo' - show a11y violations in the test UI only
      // 'error' - fail CI on a11y violations
      // 'off' - skip a11y checks entirely
      test: "todo"
    }
  },

  initialGlobals: {
    backgrounds: { value: 'dark' },
  },
  
  decorators: [
    (Story) => (
        <div style={{
          backgroundColor: 'rgb(35,35,35)',
          fontFamily: '"Cascadia Mono", monospace',
          color: 'white',
        }}>
          <Story />
        </div>
    )
  ]
};

export default preview;