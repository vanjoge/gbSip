export const superior = {
  list: 'Superior/GetSuperiorList',
  info: 'Superior/GetSuperior',
  create: 'Superior/CreateSuperior',
  update: 'Superior/UpdateSuperior',
  delete: 'Superior/DeleteSuperiors',
} as const;
export const values = Object.values(superior);

export type SuperiorPerms = typeof values[number];

export default superior;
