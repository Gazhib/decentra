import { Outlet } from "react-router";
import Header from "./widget/Header";

export default function RootLayout() {
  return (
    <>
      <Header />
      <Outlet />
    </>
  );
}
