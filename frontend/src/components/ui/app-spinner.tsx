import { Spinner, type SpinnerProps } from "flowbite-react";

export type AppSpinnerProps = SpinnerProps;

export function AppSpinner(props: AppSpinnerProps) {
  return <Spinner {...props} />;
}
