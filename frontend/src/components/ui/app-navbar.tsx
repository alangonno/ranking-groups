import { Navbar, type NavbarProps } from "flowbite-react";

export type AppNavbarProps = NavbarProps;

export function AppNavbar(props: AppNavbarProps) {
  return <Navbar {...props} />;
}
