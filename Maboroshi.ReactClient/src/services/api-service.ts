import { v4 as uuidv4 } from 'uuid';
import { Environment } from '../state/atoms';

export const fetchInitialState = async () => {
  const response = await fetch('$$$SYSTEM$$$/environments');
  if (response.ok) {
    const data = await response.json();
    return addClientIds(data);
  }

  if (!response.ok) {
    throw new Error(await readError(response));
  }
};

export const updateEnvironments = async (environments: Environment[]) => {
  const response = await fetch('$$$SYSTEM$$$/environments', {
    method: 'PUT',
    body: JSON.stringify(environments),
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }
}

// the API reports an invalid configuration as ProblemDetails with the reason in "detail"
async function readError(response: Response): Promise<string> {
  const text = await response.text();
  const fallback = text || `Server responded with ${response.status} ${response.statusText}`;
  try {
    return JSON.parse(text).detail ?? fallback;
  } catch {
    return fallback;
  }
}

function addClientIds<T>(data: T): T {
  if (Array.isArray(data)) {
    return data.map(item => addClientIds(item)) as T;
  } else if (typeof data === 'object' && data !== null) {
    const newObj: any = { ...data, id: uuidv4() };

    for (const key in newObj) {
      if (Array.isArray(newObj[key]) || typeof newObj[key] === 'object') {
        newObj[key] = addClientIds(newObj[key]);
      }
    }
    return newObj;
  }

  return data;
}