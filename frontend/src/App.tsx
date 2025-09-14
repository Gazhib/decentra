import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import HomePage from "./routes/HomePage";
import AuthPage, { action as authAction } from "./routes/AuthPage";
import ErrorPage from "./error/ErrorPage";
import VehicleApprovalPage, {
  action as vehicleUploadAction,
} from "./routes/VehicleApprovalPage";
import RootLayout from "./RootLayout";
import AboutPage from "./routes/AboutPage";
import AppealPage from "./routes/AppealPage";
import ProtectedRoutes from "./util/ProtectedRoutes";
import AccountPage from "./routes/AccountPage";
import AuthProtectedRoutes from "./util/AuthRoutes";

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
          path: "/about",
          element: <AboutPage />,
        },
        {
          path: "/",
          element: <ProtectedRoutes />,
          children: [
            {
              path: "/car-verification",
              element: <VehicleApprovalPage />,
              action: vehicleUploadAction,
            },
            { path: "/appeal", element: <AppealPage /> },
            {
              path: "/account",
              element: <AccountPage />,
            },
          ],
        },
        {
          path: "/",
          element: <AuthProtectedRoutes />,
          children: [
            {
              path: "/auth",
              element: <AuthPage />,
              action: authAction,
            },
          ],
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
