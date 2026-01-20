import { api } from '@/lib/apiClient';
import { z } from 'astro/zod';
import { ActionError, defineAction } from 'astro:actions';

export const login = defineAction({
  accept: 'form',
  input: z.object({
    username: z.string().min(1, 'Username is required'),
    password: z.string().min(1, 'Password is required'),
  }),
  handler: async (input) => {
    try {
      // 2. Envolvemos la llamada en un try/catch
      const res = await api.api.authLoginUalCreate({
        username: input.username,
        password: input.password,
      });

      return { success: true, data: res.data };
    } catch (err: any) {
      console.error('Error en login:', err);

      const message =
        err.error?.message || err.message || 'Credenciales inválidas';

      const code =
        err.status === 401 ? 'UNAUTHORIZED' : 'INTERNAL_SERVER_ERROR';

      throw new ActionError({
        code: code,
        message:
          typeof message === 'string' ? message : JSON.stringify(message),
      });
    }
  },
});
