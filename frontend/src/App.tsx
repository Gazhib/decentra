import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import HomePage from "./routes/HomePage";
import AuthPage, { action as authAction } from "./routes/AuthPage";
import ErrorPage from "./error/ErrorPage";
import VehicleApprovalPage from "./routes/VehicleApprovalPage";
import RootLayout from "./RootLayout";

function App() {
  const queryClient = new QueryClient();

  const router = createBrowserRouter([
    {
      path: "/",
      element: <RootLayout />,
      errorElement: <ErrorPage />,
      children: [
        {
          path: "/",
          element: <HomePage />,
        },
        {
          path: "/auth",
          element: <AuthPage />,
          action: authAction,
        },
        {
          path: "/car-verification",
          element: <VehicleApprovalPage />,
        },
      ],
    },
  ]);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

export default App;
