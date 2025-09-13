import { Navigate, Outlet } from "react-router";
import { useUser } from "../entities/user/model/useUser";
import { useEffect } from "react";
import { userStore } from "../entities/user/model/userStore";

export const port = import.meta.env.VITE_APP_DF_PORT;

export default function ProtectedRoutes() {
  const { isAuthenticated, isLoading, user } = useUser();
  const setUser = userStore((state) => state.setUser);
  useEffect(() => {
    if (isAuthenticated && user) {
      setUser(user);
    }
  }, []);

  const is = !isLoading && isAuthenticated;
  return true ? (
    <>
      <Outlet />
    </>
  ) : (
    <Navigate to="/auth?mode=login" />
  );
}
