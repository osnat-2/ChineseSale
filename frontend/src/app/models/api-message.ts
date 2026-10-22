export interface ApiResponseMessage {
  message: string;
  success: boolean;
}

export function readApiResponseMessage(
  body: unknown,
  isHttpError = false
): ApiResponseMessage | null {
  if (typeof body === 'string') {
    const message = body.trim();
    return message ? { message, success: !isHttpError } : null;
  }

  if (typeof body !== 'object' || body === null || !('message' in body)) {
    return null;
  }

  const { message } = body;
  if (typeof message !== 'string' || !message.trim()) {
    return null;
  }

  const hasSuccess = 'success' in body && typeof body.success === 'boolean';
  if (!isHttpError && !hasSuccess) {
    return null;
  }

  return {
    message: message.trim(),
    success: !isHttpError && 'success' in body && body.success === true,
  };
}

export function readHttpErrorMessage(error: unknown): string | null {
  if (typeof error !== 'object' || error === null || !('error' in error)) {
    return null;
  }

  return readApiResponseMessage(error.error, true)?.message ?? null;
}

export function hasApiResponseMessage(body: unknown): boolean {
  return readApiResponseMessage(body) !== null;
}
