import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import HomePage from "./routes/Home";
import AuthPage, { action as authAction } from "./routes/AuthPage";
import ErrorPage from "./error/ErrorPage";

function App() {
  const queryClient = new QueryClient();

  const router = createBrowserRouter([
    {
      path: "/",
      element: <HomePage />,
      children: [],
    },
    {
      path: "/auth",
      element: <AuthPage />,
      errorElement: <ErrorPage />,
      action: authAction,
    },
  ]);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

export default App;
