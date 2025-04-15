import { v4 as uuidv4 } from 'uuid';
import { Environment } from '../state/atoms';

export const fetchInitialState = async () => {
    const response = await fetch('$$$SYSTEM$$$/environments');
    if (response.ok) {
        const data = await response.json();
        return addClientIds(data);
    }
};

export const updateEnvironments = async (environments: Environment[]) => {
  return await fetch('$$$SYSTEM$$$/environments', {
    method: 'PUT',
    body: JSON.stringify(environments),
    headers: {
      'Content-Type': 'application/json'
    }
  });
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