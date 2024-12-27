import { Environment } from "../state/atoms";

const mockData: Environment[] = [
  {
    id: "1",
    name: "Development",
    routes: [
      {
        id: "1",
        path: "/api/users",
        methods: ["GET", "POST"],
        enabled: true,
        responses: [
          {
            id: "1",
            name: "Success",
            statusCode: "200",
            body: '{"message": "Success"}',
            headers: [
              { id: "1", key: "Content-Type", value: "application/json" },
            ],
            rules: [],
          },
        ],
      },
    ],
  },
  {
    id: "2",
    name: "Production",
    routes: [],
  },
];

export const fetchInitialState = (): Promise<Environment[]> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(mockData);
    }, 1000);
  });
};
