import { Link } from 'react-router';

export default function NotFound() {
  return (
    <>
      <h1>Nie znaleziono strony</h1>
      <p>Strona o podanym adresie nie istnieje.</p>
      <p><Link to="/">Wróć do strony głównej</Link></p>
    </>
  );
}
