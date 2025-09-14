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
import AppealPage, { action as appealAction } from "./routes/AppealPage";
import ProtectedRoutes from "./util/ProtectedRoutes";
import AccountPage from "./routes/AccountPage";
import AuthProtectedRoutes from "./util/AuthRoutes";
import AdminPage from "./routes/AdminPage";
import AdminAppealPage from "./routes/AdminAppealPage";

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
            {
              path: "/appeal",
              element: <AppealPage />,
              action: appealAction,
            },
            {
              path: "/account/:userId",
              element: <AccountPage />,
            },
            {
              path: "/admin",
              element: <AdminPage />,
            },
            {
              path: "/admin/appeals/:appealId",
              element: <AdminAppealPage />,
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
