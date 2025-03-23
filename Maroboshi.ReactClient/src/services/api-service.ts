import { v4 as uuidv4 } from 'uuid';

export const fetchInitialState = async () => {
    const response = await fetch('$$$SYSTEM$$$/environments');
    if (response.ok) {
        const data = await response.json();
        console.log(addClientIds(data));
        return addClientIds(data);
    }
};

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