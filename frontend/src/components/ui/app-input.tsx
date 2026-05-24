import { TextInput, type TextInputProps } from "flowbite-react";

export type AppInputProps = TextInputProps;

export function AppInput(props: AppInputProps) {
  return <TextInput {...props} />;
}
