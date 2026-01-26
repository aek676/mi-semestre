import { ActionError, defineAction } from 'astro:actions';

export const logout = defineAction({
  accept: 'form',
  handler: async (_input, context) => {
    try {
      context.cookies.set('bb_session', '', {
        httpOnly: true,
        secure: import.meta.env.PROD,
        sameSite: 'lax',
        path: '/',
        maxAge: 0,
      });

      return {
        success: true,
        message: 'Logged out',
      };
    } catch (err: any) {
      console.error('[Logout Action Error]:', err);
      throw new ActionError({
        code: 'INTERNAL_SERVER_ERROR',
        message: 'An unexpected error occurred while logging out.',
      });
    }
  },
});
