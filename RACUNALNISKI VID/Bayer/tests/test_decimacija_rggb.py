import bayer
import unittest
import numpy as np

class Test(unittest.TestCase):
    def test_decimacija_rggb(self):
        podatki = np.array(
            [[110, 150, 220, 230, 110, 180],
             [120, 160, 210, 240, 150, 140],
             [130, 170, 200, 250, 120, 170],
             [140, 180, 190, 250, 160, 130]], dtype=np.uint8)

        resitev_rggb = np.array(
            [[[110, (150+120)//2, 160], 
              [220, (230+210)//2, 240], 
              [110, (180+150)//2, 140]], 
             [[130, (170+140)//2, 180], 
              [200, (250+190)//2, 250], 
              [120, (170+160)//2, 130]]], dtype=np.uint8)

        rezultat = bayer.bayer_v_rgb(podatki, 'RGGB')
        self.assertIsInstance(rezultat, np.ndarray)
        self.assertEqual(rezultat.dtype, np.uint8)
        np.testing.assert_array_equal(rezultat, resitev_rggb)

if __name__ == '__main__':
    unittest.main(verbosity=2)
