import bayer
import unittest
import numpy as np


class Test(unittest.TestCase):
    def test_interpolacija_gbrg(self):
        podatki = np.array([[2, 6, 2, 8],
                            [4, 8, 6, 6],
                            [8, 2, 4, 4],
                            [6, 4, 8, 2]], dtype=np.uint8)

        resitev_gbrg = np.zeros((4,4,3), dtype=np.uint8)
        resitev_gbrg[:, :, 0] = [[4, 5, 6, 6], 
                                 [4, 5, 6, 6],
                                 [5, 6, 7, 7],
                                 [6, 7, 8, 8]]
        resitev_gbrg[:, :, 1] = [[2, 4, 2, 4], 
                                 [6, 8, 5, 6],
                                 [8, 6, 4, 4],
                                 [6, 4, 3, 2]]
        resitev_gbrg[:, :, 2] = [[6, 6, 7, 8], 
                                 [4, 4, 5, 6],
                                 [2, 2, 3, 4],
                                 [2, 2, 3, 4]]

        rezultat = bayer.bayer_v_rgb(podatki, 'GBRG', True)
        self.assertIsInstance(rezultat, np.ndarray)
        self.assertEqual(rezultat.dtype, np.uint8)
        np.testing.assert_array_equal(rezultat, resitev_gbrg)

if __name__ == '__main__':
    unittest.main()
