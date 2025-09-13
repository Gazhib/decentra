import { Outlet } from "react-router";
import Header from "./widget/Header";
import Footer from "./widget/Footer";

export default function RootLayout() {
  return (
    <>
      <Header />
      <Outlet />
      <Footer />
    </>
  );
}
